using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;

namespace Bibliotheque.Infrastructure.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmpruntService _empruntService;

        public ReservationService(IUnitOfWork unitOfWork, IEmpruntService empruntService)
        {
            _unitOfWork = unitOfWork;
            _empruntService = empruntService;
        }

        public async Task<(bool Succes, string Message, int? IdReservation, int Position)> ReserverAsync(
            int idLivre, int idUtilisateur)
        {
            // Vérifier que le livre existe
            var livre = await _unitOfWork.Livres.GetByIdAsync(idLivre);
            if (livre == null || !livre.Actif)
            {
                return (false, "Livre non trouvé.", null, 0);
            }

            // Vérifier l'utilisateur
            var utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(idUtilisateur);
            if (utilisateur == null || !utilisateur.Actif || utilisateur.EstBloque)
            {
                return (false, "Votre compte ne permet pas d'effectuer des réservations.", null, 0);
            }

            // Vérifier si l'utilisateur a déjà emprunté ce livre
            if (await _unitOfWork.Emprunts.ADejaEmprunteAsync(idUtilisateur, idLivre))
            {
                return (false, "Vous avez déjà emprunté ce livre.", null, 0);
            }

            // Vérifier si l'utilisateur a déjà réservé ce livre
            if (await _unitOfWork.Reservations.ADejaReserveAsync(idUtilisateur, idLivre))
            {
                return (false, "Vous avez déjà une réservation en cours pour ce livre.", null, 0);
            }

            // Si le livre est disponible, l'utilisateur devrait emprunter directement
            if (livre.StockDisponible > 0)
            {
                return (false, "Ce livre est disponible, vous pouvez l'emprunter directement.", null, 0);
            }

            // Calculer la position dans la file
            var reservationsExistantes = await _unitOfWork.Reservations.GetByLivreAsync(idLivre, true);
            var position = reservationsExistantes.Count() + 1;

            // Créer la réservation
            var reservation = new Reservation
            {
                IdLivre = idLivre,
                IdUtilisateur = idUtilisateur,
                DateReservation = DateTime.Now,
                DateExpiration = DateTime.Now.AddDays(30), // Expiration si non disponible après 30 jours
                PositionFile = position,
                Statut = "EnAttente"
            };

            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Réservation effectuée. Vous êtes en position {position} dans la file d'attente.", reservation.IdReservation, position);
        }

        public async Task<(bool Succes, string Message)> AnnulerReservationAsync(int idReservation)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdWithDetailsAsync(idReservation);

            if (reservation == null)
            {
                return (false, "Réservation non trouvée.");
            }

            if (reservation.Statut != "EnAttente" && reservation.Statut != "Disponible")
            {
                return (false, "Cette réservation ne peut pas être annulée.");
            }

            var idLivre = reservation.IdLivre;
            reservation.Statut = "Annulee";
            await _unitOfWork.Reservations.UpdateAsync(reservation);

            // Recalculer les positions dans la file
            await _unitOfWork.Reservations.RecalculerPositionsFileAsync(idLivre);

            await _unitOfWork.SaveChangesAsync();

            return (true, "Réservation annulée avec succès.");
        }

        public async Task<(bool Succes, string Message, int? IdEmprunt)> ConvertirEnEmpruntAsync(int idReservation)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdWithDetailsAsync(idReservation);

            if (reservation == null)
            {
                return (false, "Réservation non trouvée.", null);
            }

            if (reservation.Statut != "Disponible")
            {
                return (false, "Cette réservation n'est pas disponible pour être convertie en emprunt.", null);
            }

            // Vérifier que la réservation n'est pas expirée
            if (DateTime.Now > reservation.DateExpiration)
            {
                reservation.Statut = "Annulee";
                await _unitOfWork.Reservations.UpdateAsync(reservation);
                await _unitOfWork.SaveChangesAsync();
                return (false, "Cette réservation a expiré.", null);
            }

            // Effectuer l'emprunt
            var (succes, message, idEmprunt) = await _empruntService.EffectuerEmpruntAsync(
                reservation.IdLivre, reservation.IdUtilisateur);

            if (succes)
            {
                // La réservation est automatiquement marquée comme convertie dans EffectuerEmpruntAsync
                return (true, "Réservation convertie en emprunt avec succès.", idEmprunt);
            }

            return (false, message, null);
        }

        public async Task ExpirerReservationsAsync()
        {
            var reservationsExpirees = await _unitOfWork.Reservations.GetExpireesAsync();

            foreach (var reservation in reservationsExpirees)
            {
                reservation.Statut = "Annulee";
                await _unitOfWork.Reservations.UpdateAsync(reservation);

                // Recalculer les positions
                await _unitOfWork.Reservations.RecalculerPositionsFileAsync(reservation.IdLivre);

                // Notifier le prochain en file d'attente si le livre est disponible
                var livre = await _unitOfWork.Livres.GetByIdAsync(reservation.IdLivre);
                if (livre != null && livre.StockDisponible > 0)
                {
                    var prochaineReservation = await _unitOfWork.Reservations.GetProchaineEnAttenteAsync(reservation.IdLivre);
                    if (prochaineReservation != null)
                    {
                        prochaineReservation.Statut = "Disponible";
                        prochaineReservation.DateNotification = DateTime.Now;
                        prochaineReservation.DateExpiration = DateTime.Now.AddDays(3);
                        await _unitOfWork.Reservations.UpdateAsync(prochaineReservation);

                        await _unitOfWork.Notifications.CreerNotificationDisponibiliteAsync(
                            prochaineReservation.IdUtilisateur,
                            livre.IdLivre,
                            livre.Titre);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
