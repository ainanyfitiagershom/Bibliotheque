using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;

namespace Bibliotheque.Infrastructure.Services
{
    public class EmpruntService : IEmpruntService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const decimal PENALITE_PAR_JOUR = 0.50m;

        public EmpruntService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Succes, string Message, int? IdEmprunt)> EffectuerEmpruntAsync(
            int idLivre, int idUtilisateur, int dureeJours = 14)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Vérifier que le livre existe et est disponible
                var livre = await _unitOfWork.Livres.GetByIdAsync(idLivre);
                if (livre == null)
                {
                    return (false, "Livre non trouvé.", null);
                }

                if (livre.StockDisponible <= 0)
                {
                    return (false, "Ce livre n'est plus disponible.", null);
                }

                // Vérifier l'utilisateur
                var utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(idUtilisateur);
                if (utilisateur == null)
                {
                    return (false, "Utilisateur non trouvé.", null);
                }

                if (!utilisateur.Actif || utilisateur.EstBloque)
                {
                    return (false, "Votre compte ne permet pas d'effectuer des emprunts.", null);
                }

                // Vérifier la limite d'emprunts
                var empruntsEnCours = await _unitOfWork.Emprunts.CompterEmpruntsEnCoursAsync(idUtilisateur);
                if (empruntsEnCours >= utilisateur.NombreEmpruntsMax)
                {
                    return (false, $"Vous avez atteint votre limite de {utilisateur.NombreEmpruntsMax} emprunts simultanés.", null);
                }

                // Vérifier si l'utilisateur n'a pas déjà ce livre
                if (await _unitOfWork.Emprunts.ADejaEmprunteAsync(idUtilisateur, idLivre))
                {
                    return (false, "Vous avez déjà emprunté ce livre.", null);
                }

                // Créer l'emprunt
                var emprunt = new Emprunt
                {
                    IdLivre = idLivre,
                    IdUtilisateur = idUtilisateur,
                    DateEmprunt = DateTime.Now,
                    DateRetourPrevue = DateTime.Now.AddDays(dureeJours),
                    Statut = "EnCours",
                    NombreProlongations = 0,
                    MaxProlongations = 2,
                    Penalite = 0
                };

                await _unitOfWork.Emprunts.AddAsync(emprunt);

                // Mettre à jour le stock
                livre.StockDisponible -= 1;
                livre.NombreEmprunts += 1;
                livre.DateModification = DateTime.Now;
                await _unitOfWork.Livres.UpdateAsync(livre);

                // Si l'utilisateur avait une réservation, la marquer comme convertie
                var reservation = await _unitOfWork.Reservations
                    .FirstOrDefaultAsync(r => r.IdLivre == idLivre &&
                        r.IdUtilisateur == idUtilisateur &&
                        (r.Statut == "EnAttente" || r.Statut == "Disponible"));

                if (reservation != null)
                {
                    reservation.Statut = "Convertie";
                    await _unitOfWork.Reservations.UpdateAsync(reservation);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, "Emprunt effectué avec succès.", emprunt.IdEmprunt);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return (false, $"Erreur lors de l'emprunt : {ex.Message}", null);
            }
        }

        public async Task<(bool Succes, string Message, decimal Penalite)> EffectuerRetourAsync(int idEmprunt)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(idEmprunt);
                if (emprunt == null)
                {
                    return (false, "Emprunt non trouvé.", 0);
                }

                if (emprunt.Statut == "Termine")
                {
                    return (false, "Cet emprunt est déjà terminé.", 0);
                }

                // Calculer la pénalité si retard
                decimal penalite = 0;
                var joursRetard = (DateTime.Now.Date - emprunt.DateRetourPrevue.Date).Days;
                if (joursRetard > 0)
                {
                    penalite = joursRetard * PENALITE_PAR_JOUR;
                }

                // Mettre à jour l'emprunt
                emprunt.DateRetourEffective = DateTime.Now;
                emprunt.Statut = "Termine";
                emprunt.Penalite = penalite;
                await _unitOfWork.Emprunts.UpdateAsync(emprunt);

                // Mettre à jour le stock du livre
                var livre = await _unitOfWork.Livres.GetByIdAsync(emprunt.IdLivre);
                if (livre != null)
                {
                    livre.StockDisponible += 1;
                    livre.DateModification = DateTime.Now;
                    await _unitOfWork.Livres.UpdateAsync(livre);

                    // Notifier le premier en file d'attente
                    var prochaineReservation = await _unitOfWork.Reservations.GetProchaineEnAttenteAsync(emprunt.IdLivre);
                    if (prochaineReservation != null)
                    {
                        prochaineReservation.Statut = "Disponible";
                        prochaineReservation.DateNotification = DateTime.Now;
                        prochaineReservation.DateExpiration = DateTime.Now.AddDays(3);
                        await _unitOfWork.Reservations.UpdateAsync(prochaineReservation);

                        // Créer une notification
                        await _unitOfWork.Notifications.CreerNotificationDisponibiliteAsync(
                            prochaineReservation.IdUtilisateur,
                            livre.IdLivre,
                            livre.Titre);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var message = penalite > 0
                    ? $"Retour effectué. Pénalité de retard : {penalite:C} ({joursRetard} jours)"
                    : "Retour effectué avec succès.";

                return (true, message, penalite);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return (false, $"Erreur lors du retour : {ex.Message}", 0);
            }
        }

        public async Task<(bool Succes, string Message)> ProlongerEmpruntAsync(int idEmprunt, int nombreJours = 7)
        {
            var emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(idEmprunt);

            if (emprunt == null)
            {
                return (false, "Emprunt non trouvé.");
            }

            if (emprunt.Statut == "Termine")
            {
                return (false, "Impossible de prolonger un emprunt terminé.");
            }

            if (emprunt.NombreProlongations >= emprunt.MaxProlongations)
            {
                return (false, $"Nombre maximum de prolongations atteint ({emprunt.MaxProlongations}).");
            }

            // Vérifier s'il y a des réservations en attente
            var reservationsEnAttente = await _unitOfWork.Reservations.GetByLivreAsync(emprunt.IdLivre, true);
            if (reservationsEnAttente.Any())
            {
                return (false, "Impossible de prolonger : des réservations sont en attente pour ce livre.");
            }

            // Prolonger l'emprunt
            emprunt.DateRetourPrevue = emprunt.DateRetourPrevue.AddDays(nombreJours);
            emprunt.NombreProlongations += 1;
            emprunt.Statut = "EnCours"; // Réinitialiser si était en retard

            await _unitOfWork.Emprunts.UpdateAsync(emprunt);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Emprunt prolongé de {nombreJours} jours. Nouvelle date de retour : {emprunt.DateRetourPrevue:dd/MM/yyyy}");
        }

        public async Task DetecterRetardsAsync()
        {
            // Récupérer les emprunts en retard non encore marqués
            var empruntsEnRetard = await _unitOfWork.Emprunts
                .FindAsync(e => e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now);

            foreach (var emprunt in empruntsEnRetard)
            {
                emprunt.Statut = "EnRetard";
                await _unitOfWork.Emprunts.UpdateAsync(emprunt);

                // Charger les détails pour la notification
                var empruntDetails = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(emprunt.IdEmprunt);
                if (empruntDetails?.Livre != null)
                {
                    await _unitOfWork.Notifications.CreerNotificationRetardAsync(
                        emprunt.IdUtilisateur,
                        emprunt.IdEmprunt,
                        empruntDetails.Livre.Titre);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
