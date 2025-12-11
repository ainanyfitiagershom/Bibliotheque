using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des notifications
    /// </summary>
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>
        /// Obtenir les notifications d'un utilisateur
        /// </summary>
        Task<IEnumerable<Notification>> GetByUtilisateurAsync(int idUtilisateur, bool nonLuesUniquement = false);

        /// <summary>
        /// Obtenir le nombre de notifications non lues
        /// </summary>
        Task<int> CompterNonLuesAsync(int idUtilisateur);

        /// <summary>
        /// Marquer une notification comme lue
        /// </summary>
        Task MarquerCommeLueAsync(int idNotification);

        /// <summary>
        /// Marquer toutes les notifications d'un utilisateur comme lues
        /// </summary>
        Task MarquerToutesCommeLuesAsync(int idUtilisateur);

        /// <summary>
        /// Supprimer les vieilles notifications (plus de X jours)
        /// </summary>
        Task SupprimerAnciennesAsync(int joursAnciennete = 30);

        /// <summary>
        /// Créer une notification de bienvenue
        /// </summary>
        Task CreerNotificationBienvenueAsync(int idUtilisateur);

        /// <summary>
        /// Créer une notification de retard
        /// </summary>
        Task CreerNotificationRetardAsync(int idUtilisateur, int idEmprunt, string titreLivre);

        /// <summary>
        /// Créer une notification de disponibilité
        /// </summary>
        Task CreerNotificationDisponibiliteAsync(int idUtilisateur, int idLivre, string titreLivre);
    }
}
