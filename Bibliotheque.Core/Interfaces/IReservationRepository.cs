using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface spécifique pour le repository des réservations
    /// </summary>
    public interface IReservationRepository : IRepository<Reservation>
    {
        /// <summary>
        /// Obtenir une réservation avec ses relations
        /// </summary>
        Task<Reservation?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Obtenir les réservations d'un utilisateur
        /// </summary>
        Task<IEnumerable<Reservation>> GetByUtilisateurAsync(int idUtilisateur, bool activeUniquement = true);

        /// <summary>
        /// Obtenir les réservations pour un livre
        /// </summary>
        Task<IEnumerable<Reservation>> GetByLivreAsync(int idLivre, bool enAttenteUniquement = true);

        /// <summary>
        /// Obtenir la position dans la file d'attente
        /// </summary>
        Task<int> GetPositionFileAsync(int idLivre, int idUtilisateur);

        /// <summary>
        /// Obtenir la prochaine réservation en attente pour un livre
        /// </summary>
        Task<Reservation?> GetProchaineEnAttenteAsync(int idLivre);

        /// <summary>
        /// Vérifier si un utilisateur a déjà réservé un livre
        /// </summary>
        Task<bool> ADejaReserveAsync(int idUtilisateur, int idLivre);

        /// <summary>
        /// Obtenir les réservations expirées
        /// </summary>
        Task<IEnumerable<Reservation>> GetExpireesAsync();

        /// <summary>
        /// Mettre à jour les positions dans la file après une annulation
        /// </summary>
        Task RecalculerPositionsFileAsync(int idLivre);
    }
}
