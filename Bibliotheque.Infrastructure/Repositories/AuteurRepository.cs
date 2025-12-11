using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class AuteurRepository : Repository<Auteur>, IAuteurRepository
    {
        public AuteurRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Auteur?> GetByIdWithLivresAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Livres.Where(l => l.Actif))
                .FirstOrDefaultAsync(a => a.IdAuteur == id);
        }

        public async Task<IEnumerable<Auteur>> RechercherAsync(string terme)
        {
            var termeNormalise = terme.ToLower();
            return await _dbSet
                .Where(a => a.Actif &&
                    (a.Nom.ToLower().Contains(termeNormalise) ||
                     (a.Prenom != null && a.Prenom.ToLower().Contains(termeNormalise))))
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .ToListAsync();
        }

        public async Task<IEnumerable<Auteur>> GetPopulairesAsync(int nombre = 10)
        {
            return await _dbSet
                .Include(a => a.Livres)
                .Where(a => a.Actif)
                .OrderByDescending(a => a.Livres.Sum(l => l.NombreEmprunts))
                .Take(nombre)
                .ToListAsync();
        }

        public async Task<bool> PeutEtreSupprime(int id)
        {
            return !await _context.Livres.AnyAsync(l => l.IdAuteur == id && l.Actif);
        }

        public async Task<Auteur> GetOrCreateAsync(string nom, string? prenom = null)
        {
            var auteur = await _dbSet
                .FirstOrDefaultAsync(a => a.Nom.ToLower() == nom.ToLower() &&
                    (prenom == null || (a.Prenom != null && a.Prenom.ToLower() == prenom.ToLower())));

            if (auteur != null)
                return auteur;

            auteur = new Auteur
            {
                Nom = nom,
                Prenom = prenom,
                DateCreation = DateTime.Now,
                Actif = true
            };

            await _dbSet.AddAsync(auteur);
            return auteur;
        }
    }
}
