using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Reservation?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Include(r => r.Utilisateur)
                .FirstOrDefaultAsync(r => r.IdReservation == id);
        }

        public async Task<IEnumerable<Reservation>> GetByUtilisateurAsync(int idUtilisateur, bool activeUniquement = true)
        {
            var query = _dbSet
                .Include(r => r.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Where(r => r.IdUtilisateur == idUtilisateur);

            if (activeUniquement)
            {
                query = query.Where(r => r.Statut == "EnAttente" || r.Statut == "Disponible");
            }

            return await query
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByLivreAsync(int idLivre, bool enAttenteUniquement = true)
        {
            var query = _dbSet
                .Include(r => r.Utilisateur)
                .Where(r => r.IdLivre == idLivre);

            if (enAttenteUniquement)
            {
                query = query.Where(r => r.Statut == "EnAttente");
            }

            return await query
                .OrderBy(r => r.PositionFile)
                .ToListAsync();
        }

        public async Task<int> GetPositionFileAsync(int idLivre, int idUtilisateur)
        {
            var reservation = await _dbSet
                .FirstOrDefaultAsync(r => r.IdLivre == idLivre &&
                    r.IdUtilisateur == idUtilisateur &&
                    r.Statut == "EnAttente");

            return reservation?.PositionFile ?? 0;
        }

        public async Task<Reservation?> GetProchaineEnAttenteAsync(int idLivre)
        {
            return await _dbSet
                .Include(r => r.Utilisateur)
                .Where(r => r.IdLivre == idLivre && r.Statut == "EnAttente")
                .OrderBy(r => r.PositionFile)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ADejaReserveAsync(int idUtilisateur, int idLivre)
        {
            return await _dbSet.AnyAsync(r =>
                r.IdUtilisateur == idUtilisateur &&
                r.IdLivre == idLivre &&
                (r.Statut == "EnAttente" || r.Statut == "Disponible"));
        }

        public async Task<IEnumerable<Reservation>> GetExpireesAsync()
        {
            return await _dbSet
                .Include(r => r.Livre)
                .Include(r => r.Utilisateur)
                .Where(r => r.Statut == "Disponible" && r.DateExpiration < DateTime.Now)
                .ToListAsync();
        }

        public async Task RecalculerPositionsFileAsync(int idLivre)
        {
            var reservations = await _dbSet
                .Where(r => r.IdLivre == idLivre && r.Statut == "EnAttente")
                .OrderBy(r => r.PositionFile)
                .ThenBy(r => r.DateReservation)
                .ToListAsync();

            for (int i = 0; i < reservations.Count; i++)
            {
                reservations[i].PositionFile = i + 1;
            }
        }
    }
}
