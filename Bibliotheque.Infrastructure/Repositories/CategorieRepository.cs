using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class CategorieRepository : Repository<Categorie>, ICategorieRepository
    {
        public CategorieRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Categorie?> GetByIdWithLivresAsync(int id)
        {
            return await _dbSet
                .Include(c => c.LivreCategories)
                    .ThenInclude(lc => lc.Livre)
                        .ThenInclude(l => l!.Auteur)
                .FirstOrDefaultAsync(c => c.IdCategorie == id);
        }

        public async Task<IEnumerable<Categorie>> GetAllWithCountAsync()
        {
            return await _dbSet
                .Include(c => c.LivreCategories)
                .Where(c => c.Actif)
                .OrderBy(c => c.Nom)
                .ToListAsync();
        }

        public async Task<bool> NomExisteAsync(string nom, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Nom.ToLower() == nom.ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.IdCategorie != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> PeutEtreSupprime(int id)
        {
            return !await _context.LivreCategories.AnyAsync(lc => lc.IdCategorie == id);
        }

        public async Task<Categorie> GetOrCreateAsync(string nom)
        {
            var categorie = await _dbSet
                .FirstOrDefaultAsync(c => c.Nom.ToLower() == nom.ToLower());

            if (categorie != null)
                return categorie;

            categorie = new Categorie
            {
                Nom = nom,
                DateCreation = DateTime.Now,
                Actif = true
            };

            await _dbSet.AddAsync(categorie);
            return categorie;
        }
    }
}
