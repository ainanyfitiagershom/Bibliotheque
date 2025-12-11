using System.Linq.Expressions;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Interface générique pour les opérations CRUD de base
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // Lecture
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // Écriture
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);

        // Pagination
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    }

    /// <summary>
    /// Interface pour le Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ILivreRepository Livres { get; }
        IAuteurRepository Auteurs { get; }
        ICategorieRepository Categories { get; }
        IUtilisateurRepository Utilisateurs { get; }
        IEmpruntRepository Emprunts { get; }
        IReservationRepository Reservations { get; }
        IAvisRepository Avis { get; }
        INotificationRepository Notifications { get; }
        IAdminRepository Admins { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
