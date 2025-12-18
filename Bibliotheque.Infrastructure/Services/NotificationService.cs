using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;

namespace Bibliotheque.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task EnvoyerNotificationAsync(int idUtilisateur, string type, string titre, string message, string? lien = null)
        {
            var notification = new Notification
            {
                IdUtilisateur = idUtilisateur,
                Type = type,
                Titre = titre,
                Message = message,
                Lien = lien,
                DateCreation = DateTime.Now,
                EstLue = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task EnvoyerNotificationsRetardAsync()
        {
            // Récupérer les emprunts en retard non encore notifiés aujourd'hui
            var empruntsEnRetard = await _unitOfWork.Emprunts
                .FindAsync(e => e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now);

            foreach (var emprunt in empruntsEnRetard)
            {
                // Marquer comme en retard
                emprunt.Statut = "EnRetard";
                await _unitOfWork.Emprunts.UpdateAsync(emprunt);

                // Charger les détails pour la notification
                var empruntDetails = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(emprunt.IdEmprunt);
                if (empruntDetails?.Livre != null)
                {
                    // Vérifier si une notification de retard n'a pas déjà été envoyée récemment
                    var notificationsExistantes = await _unitOfWork.Notifications
                        .GetByUtilisateurAsync(emprunt.IdUtilisateur);

                    var dejaNotifie = notificationsExistantes.Any(n =>
                        n.Type == "Retard" &&
                        n.Message.Contains($"#{emprunt.IdEmprunt}") &&
                        n.DateCreation.Date == DateTime.Today);

                    if (!dejaNotifie)
                    {
                        var joursRetard = (DateTime.Now.Date - emprunt.DateRetourPrevue.Date).Days;
                        var penalite = joursRetard * 0.50m;

                        await _unitOfWork.Notifications.CreerNotificationRetardAsync(
                            emprunt.IdUtilisateur,
                            emprunt.IdEmprunt,
                            empruntDetails.Livre.Titre);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task EnvoyerRappelsEcheanceAsync(int joursAvant = 2)
        {
            var dateEcheance = DateTime.Now.Date.AddDays(joursAvant);

            // Récupérer les emprunts qui arrivent à échéance
            var empruntsAEcheance = await _unitOfWork.Emprunts
                .FindAsync(e => e.Statut == "EnCours" &&
                               e.DateRetourPrevue.Date == dateEcheance);

            foreach (var emprunt in empruntsAEcheance)
            {
                var empruntDetails = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(emprunt.IdEmprunt);
                if (empruntDetails?.Livre != null)
                {
                    // Vérifier si un rappel n'a pas déjà été envoyé
                    var notificationsExistantes = await _unitOfWork.Notifications
                        .GetByUtilisateurAsync(emprunt.IdUtilisateur);

                    var dejaRappele = notificationsExistantes.Any(n =>
                        n.Type == "Rappel" &&
                        n.Message.Contains($"#{emprunt.IdEmprunt}") &&
                        n.DateCreation.Date >= DateTime.Today.AddDays(-1));

                    if (!dejaRappele)
                    {
                        await EnvoyerNotificationAsync(
                            emprunt.IdUtilisateur,
                            "Rappel",
                            "Rappel : échéance proche",
                            $"Votre emprunt #{emprunt.IdEmprunt} du livre \"{empruntDetails.Livre.Titre}\" arrive à échéance le {emprunt.DateRetourPrevue:dd/MM/yyyy}. Pensez à le retourner ou à demander une prolongation.",
                            $"/Emprunts/Details/{emprunt.IdEmprunt}");
                    }
                }
            }
        }

        public async Task NettoyerAnciennesNotificationsAsync(int joursAnciennete = 30)
        {
            await _unitOfWork.Notifications.SupprimerAnciennesAsync(joursAnciennete);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
