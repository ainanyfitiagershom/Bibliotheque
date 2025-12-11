using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des auteurs
    /// </summary>
    public interface IAuteurRepository : IRepository<Auteur>
    {
        /// <summary>
        /// Obtenir un auteur avec ses livres
        /// </summary>
        Task<Auteur?> GetByIdWithLivresAsync(int id);

        /// <summary>
        /// Rechercher des auteurs par nom
        /// </summary>
        Task<IEnumerable<Auteur>> RechercherAsync(string terme);

        /// <summary>
        /// Obtenir les auteurs les plus populaires (ayant le plus de livres empruntés)
        /// </summary>
        Task<IEnumerable<Auteur>> GetPopulairesAsync(int nombre = 10);

        /// <summary>
        /// Vérifier si un auteur peut être supprimé (n'a pas de livres)
        /// </summary>
        Task<bool> PeutEtreSupprime(int id);

        /// <summary>
        /// Obtenir ou créer un auteur par nom
        /// </summary>
        Task<Auteur> GetOrCreateAsync(string nom, string? prenom = null);
    }
}
