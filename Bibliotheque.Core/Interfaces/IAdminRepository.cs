using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des admins
    /// </summary>
    public interface IAdminRepository : IRepository<Admin>
    {
        /// <summary>
        /// Obtenir un admin par email
        /// </summary>
        Task<Admin?> GetByEmailAsync(string email);

        /// <summary>
        /// Vérifier si l'email existe déjà
        /// </summary>
        Task<bool> EmailExisteAsync(string email, int? excludeId = null);

        /// <summary>
        /// Mettre à jour la date de dernière connexion
        /// </summary>
        Task UpdateDerniereConnexionAsync(int id);

        /// <summary>
        /// Vérifier les identifiants de connexion
        /// </summary>
        Task<Admin?> VerifierCredentialsAsync(string email, string motDePasse);
    }
}
