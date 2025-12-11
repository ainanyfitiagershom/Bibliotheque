using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des emprunts
    /// </summary>
    public interface IEmpruntRepository : IRepository<Emprunt>
    {
        /// <summary>
        /// Obtenir un emprunt avec ses relations
        /// </summary>
        Task<Emprunt?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Obtenir tous les emprunts avec détails
        /// </summary>
        Task<IEnumerable<Emprunt>> GetAllWithDetailsAsync();

        /// <summary>
        /// Obtenir les emprunts avec filtre
        /// </summary>
        Task<PagedResultDTO<EmpruntDTO>> GetFiltreAsync(EmpruntFiltreDTO filtre);

        /// <summary>
        /// Obtenir les emprunts d'un utilisateur
        /// </summary>
        Task<IEnumerable<Emprunt>> GetByUtilisateurAsync(int idUtilisateur, bool enCoursUniquement = false);

        /// <summary>
        /// Obtenir les emprunts d'un livre
        /// </summary>
        Task<IEnumerable<Emprunt>> GetByLivreAsync(int idLivre);

        /// <summary>
        /// Obtenir les emprunts en retard
        /// </summary>
        Task<IEnumerable<Emprunt>> GetEnRetardAsync();

        /// <summary>
        /// Obtenir les emprunts qui arrivent à échéance (dans X jours)
        /// </summary>
        Task<IEnumerable<Emprunt>> GetProchesEcheanceAsync(int jours = 2);

        /// <summary>
        /// Vérifier si un utilisateur a déjà emprunté un livre (en cours)
        /// </summary>
        Task<bool> ADejaEmprunteAsync(int idUtilisateur, int idLivre);

        /// <summary>
        /// Compter les emprunts en cours d'un utilisateur
        /// </summary>
        Task<int> CompterEmpruntsEnCoursAsync(int idUtilisateur);

        /// <summary>
        /// Obtenir les statistiques d'emprunts par mois
        /// </summary>
        Task<IEnumerable<EmpruntParMoisDTO>> GetStatistiquesParMoisAsync(int nombreMois = 12);

        /// <summary>
        /// Obtenir les statistiques d'emprunts par catégorie
        /// </summary>
        Task<IEnumerable<EmpruntParCategorieDTO>> GetStatistiquesParCategorieAsync();
    }
}
