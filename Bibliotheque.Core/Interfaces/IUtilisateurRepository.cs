using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des utilisateurs
    /// </summary>
    public interface IUtilisateurRepository : IRepository<Utilisateur>
    {
        /// <summary>
        /// Obtenir un utilisateur par email
        /// </summary>
        Task<Utilisateur?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtenir un utilisateur avec ses emprunts et réservations
        /// </summary>
        Task<Utilisateur?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Vérifier si l'email existe déjà
        /// </summary>
        Task<bool> EmailExisteAsync(string email, int? excludeId = null);

        /// <summary>
        /// Obtenir les utilisateurs avec emprunts en retard
        /// </summary>
        Task<IEnumerable<Utilisateur>> GetAvecRetardsAsync();

        /// <summary>
        /// Obtenir les utilisateurs les plus actifs
        /// </summary>
        Task<IEnumerable<Utilisateur>> GetPlusActifsAsync(int nombre = 10);

        /// <summary>
        /// Rechercher des utilisateurs
        /// </summary>
        Task<IEnumerable<Utilisateur>> RechercherAsync(string terme);

        /// <summary>
        /// Bloquer un utilisateur
        /// </summary>
        Task BloquerAsync(int id, string raison);

        /// <summary>
        /// Débloquer un utilisateur
        /// </summary>
        Task DebloquerAsync(int id);

        /// <summary>
        /// Mettre à jour la date de dernière connexion
        /// </summary>
        Task UpdateDerniereConnexionAsync(int id);
    }
}
