using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class AdminRepository : Repository<Admin>, IAdminRepository
    {
        public AdminRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Admin?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower() && a.Actif);
        }

        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(a => a.Email.ToLower() == email.ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.IdAdmin != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task UpdateDerniereConnexionAsync(int id)
        {
            var admin = await _dbSet.FindAsync(id);
            if (admin != null)
            {
                admin.DerniereConnexion = DateTime.Now;
            }
        }

        public async Task<Admin?> VerifierCredentialsAsync(string email, string motDePasse)
        {
            var admin = await GetByEmailAsync(email);
            if (admin == null)
                return null;

            // Vérification du mot de passe avec BCrypt
            if (BCrypt.Net.BCrypt.Verify(motDePasse, admin.MotDePasseHash))
            {
                await UpdateDerniereConnexionAsync(admin.IdAdmin);
                return admin;
            }

            return null;
        }
    }
}
