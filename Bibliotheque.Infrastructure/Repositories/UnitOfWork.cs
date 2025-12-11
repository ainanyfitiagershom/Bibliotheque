using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bibliotheque.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du pattern Unit of Work pour coordonner les repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BibliothequeDbContext _context;
        private IDbContextTransaction? _transaction;

        private ILivreRepository? _livres;
        private IAuteurRepository? _auteurs;
        private ICategorieRepository? _categories;
        private IUtilisateurRepository? _utilisateurs;
        private IEmpruntRepository? _emprunts;
        private IReservationRepository? _reservations;
        private IAvisRepository? _avis;
        private INotificationRepository? _notifications;
        private IAdminRepository? _admins;

        public UnitOfWork(BibliothequeDbContext context)
        {
            _context = context;
        }

        public ILivreRepository Livres =>
            _livres ??= new LivreRepository(_context);

        public IAuteurRepository Auteurs =>
            _auteurs ??= new AuteurRepository(_context);

        public ICategorieRepository Categories =>
            _categories ??= new CategorieRepository(_context);

        public IUtilisateurRepository Utilisateurs =>
            _utilisateurs ??= new UtilisateurRepository(_context);

        public IEmpruntRepository Emprunts =>
            _emprunts ??= new EmpruntRepository(_context);

        public IReservationRepository Reservations =>
            _reservations ??= new ReservationRepository(_context);

        public IAvisRepository Avis =>
            _avis ??= new AvisRepository(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        public IAdminRepository Admins =>
            _admins ??= new AdminRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
