using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetByUtilisateurAsync(int idUtilisateur, bool nonLuesUniquement = false)
        {
            var query = _dbSet.Where(n => n.IdUtilisateur == idUtilisateur);

            if (nonLuesUniquement)
            {
                query = query.Where(n => !n.EstLue);
            }

            return await query
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public async Task<int> CompterNonLuesAsync(int idUtilisateur)
        {
            return await _dbSet.CountAsync(n =>
                n.IdUtilisateur == idUtilisateur && !n.EstLue);
        }

        public async Task MarquerCommeLueAsync(int idNotification)
        {
            var notification = await _dbSet.FindAsync(idNotification);
            if (notification != null)
            {
                notification.EstLue = true;
                notification.DateLecture = DateTime.Now;
            }
        }

        public async Task MarquerToutesCommeLuesAsync(int idUtilisateur)
        {
            var notifications = await _dbSet
                .Where(n => n.IdUtilisateur == idUtilisateur && !n.EstLue)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.EstLue = true;
                notification.DateLecture = DateTime.Now;
            }
        }

        public async Task SupprimerAnciennesAsync(int joursAnciennete = 30)
        {
            var dateLimite = DateTime.Now.AddDays(-joursAnciennete);
            var anciennes = await _dbSet
                .Where(n => n.EstLue && n.DateCreation < dateLimite)
                .ToListAsync();

            _dbSet.RemoveRange(anciennes);
        }

        public async Task CreerNotificationBienvenueAsync(int idUtilisateur)
        {
            var notification = new Notification
            {
                IdUtilisateur = idUtilisateur,
                Type = "Bienvenue",
                Titre = "Bienvenue à la bibliothèque !",
                Message = "Votre compte a été créé avec succès. Vous pouvez maintenant emprunter des livres, faire des réservations et bien plus encore !",
                Lien = "/",
                DateCreation = DateTime.Now
            };

            await _dbSet.AddAsync(notification);
        }

        public async Task CreerNotificationRetardAsync(int idUtilisateur, int idEmprunt, string titreLivre)
        {
            var notification = new Notification
            {
                IdUtilisateur = idUtilisateur,
                Type = "Retard",
                Titre = "Retard de retour",
                Message = $"Le livre \"{titreLivre}\" devait être retourné. Veuillez le retourner dès que possible pour éviter des pénalités supplémentaires.",
                Lien = $"/Emprunts/Details/{idEmprunt}",
                DateCreation = DateTime.Now
            };

            await _dbSet.AddAsync(notification);
        }

        public async Task CreerNotificationDisponibiliteAsync(int idUtilisateur, int idLivre, string titreLivre)
        {
            var notification = new Notification
            {
                IdUtilisateur = idUtilisateur,
                Type = "Disponibilite",
                Titre = "Livre disponible !",
                Message = $"Le livre \"{titreLivre}\" que vous avez réservé est maintenant disponible. Vous avez 3 jours pour venir le récupérer.",
                Lien = $"/Livres/Details/{idLivre}",
                DateCreation = DateTime.Now
            };

            await _dbSet.AddAsync(notification);
        }
    }
}
