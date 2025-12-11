using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class AvisRepository : Repository<Avis>, IAvisRepository
    {
        public AvisRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Avis>> GetByLivreAsync(int idLivre, bool approuvesUniquement = true)
        {
            var query = _dbSet
                .Include(a => a.Utilisateur)
                .Where(a => a.IdLivre == idLivre);

            if (approuvesUniquement)
            {
                query = query.Where(a => a.Approuve);
            }

            return await query
                .OrderByDescending(a => a.DateAvis)
                .ToListAsync();
        }

        public async Task<IEnumerable<Avis>> GetByUtilisateurAsync(int idUtilisateur)
        {
            return await _dbSet
                .Include(a => a.Livre)
                .Where(a => a.IdUtilisateur == idUtilisateur)
                .OrderByDescending(a => a.DateAvis)
                .ToListAsync();
        }

        public async Task<IEnumerable<Avis>> GetEnAttenteModerationAsync()
        {
            return await _dbSet
                .Include(a => a.Livre)
                .Include(a => a.Utilisateur)
                .Where(a => !a.Approuve)
                .OrderBy(a => a.DateAvis)
                .ToListAsync();
        }

        public async Task<bool> ADejaCommenteAsync(int idUtilisateur, int idLivre)
        {
            return await _dbSet.AnyAsync(a =>
                a.IdUtilisateur == idUtilisateur &&
                a.IdLivre == idLivre);
        }

        public async Task<decimal> CalculerNoteMoyenneAsync(int idLivre)
        {
            var moyenne = await _dbSet
                .Where(a => a.IdLivre == idLivre && a.Approuve)
                .AverageAsync(a => (decimal?)a.Note);

            return moyenne ?? 0;
        }
    }
}
