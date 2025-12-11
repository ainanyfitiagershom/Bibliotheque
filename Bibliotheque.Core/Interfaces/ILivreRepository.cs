using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des livres
    /// </summary>
    public interface ILivreRepository : IRepository<Livre>
    {
        /// <summary>
        /// Obtenir un livre avec ses relations (Auteur, Categories)
        /// </summary>
        Task<Livre?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Obtenir tous les livres avec leurs relations
        /// </summary>
        Task<IEnumerable<Livre>> GetAllWithDetailsAsync();

        /// <summary>
        /// Recherche avancée de livres avec pagination
        /// </summary>
        Task<PagedResultDTO<LivreDTO>> RechercherAsync(LivreRechercheDTO recherche);

        /// <summary>
        /// Obtenir les livres par catégorie
        /// </summary>
        Task<IEnumerable<Livre>> GetByCategorieAsync(int idCategorie);

        /// <summary>
        /// Obtenir les livres par auteur
        /// </summary>
        Task<IEnumerable<Livre>> GetByAuteurAsync(int idAuteur);

        /// <summary>
        /// Obtenir les livres disponibles
        /// </summary>
        Task<IEnumerable<Livre>> GetDisponiblesAsync();

        /// <summary>
        /// Obtenir les nouveautés (ajoutés récemment)
        /// </summary>
        Task<IEnumerable<Livre>> GetNouveautesAsync(int nombre = 10);

        /// <summary>
        /// Obtenir les livres les plus populaires
        /// </summary>
        Task<IEnumerable<Livre>> GetPopulairesAsync(int nombre = 10);

        /// <summary>
        /// Obtenir les livres les mieux notés
        /// </summary>
        Task<IEnumerable<Livre>> GetMieuxNotesAsync(int nombre = 10);

        /// <summary>
        /// Recherche rapide (pour auto-complétion)
        /// </summary>
        Task<IEnumerable<LivreDTO>> RechercheRapideAsync(string terme, int limite = 5);

        /// <summary>
        /// Vérifier si l'ISBN existe déjà
        /// </summary>
        Task<bool> IsbnExisteAsync(string isbn, int? excludeId = null);

        /// <summary>
        /// Mettre à jour les catégories d'un livre
        /// </summary>
        Task UpdateCategoriesAsync(int idLivre, List<int> categorieIds);

        /// <summary>
        /// Mettre à jour la note moyenne d'un livre
        /// </summary>
        Task UpdateNoteMoyenneAsync(int idLivre);
    }
}
