using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des avis
    /// </summary>
    public interface IAvisRepository : IRepository<Avis>
    {
        /// <summary>
        /// Obtenir les avis d'un livre
        /// </summary>
        Task<IEnumerable<Avis>> GetByLivreAsync(int idLivre, bool approuvesUniquement = true);

        /// <summary>
        /// Obtenir les avis d'un utilisateur
        /// </summary>
        Task<IEnumerable<Avis>> GetByUtilisateurAsync(int idUtilisateur);

        /// <summary>
        /// Obtenir les avis en attente de modération
        /// </summary>
        Task<IEnumerable<Avis>> GetEnAttenteModerationAsync();

        /// <summary>
        /// Vérifier si un utilisateur a déjà donné un avis sur un livre
        /// </summary>
        Task<bool> ADejaCommenteAsync(int idUtilisateur, int idLivre);

        /// <summary>
        /// Calculer la note moyenne d'un livre
        /// </summary>
        Task<decimal> CalculerNoteMoyenneAsync(int idLivre);
    }
}
