using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des catégories
    /// </summary>
    public interface ICategorieRepository : IRepository<Categorie>
    {
        /// <summary>
        /// Obtenir une catégorie avec ses livres
        /// </summary>
        Task<Categorie?> GetByIdWithLivresAsync(int id);

        /// <summary>
        /// Obtenir toutes les catégories avec le nombre de livres
        /// </summary>
        Task<IEnumerable<Categorie>> GetAllWithCountAsync();

        /// <summary>
        /// Vérifier si le nom existe déjà
        /// </summary>
        Task<bool> NomExisteAsync(string nom, int? excludeId = null);

        /// <summary>
        /// Vérifier si une catégorie peut être supprimée
        /// </summary>
        Task<bool> PeutEtreSupprime(int id);

        /// <summary>
        /// Obtenir ou créer une catégorie par nom
        /// </summary>
        Task<Categorie> GetOrCreateAsync(string nom);
    }
}
