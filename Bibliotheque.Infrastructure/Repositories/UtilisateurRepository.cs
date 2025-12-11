using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class UtilisateurRepository : Repository<Utilisateur>, IUtilisateurRepository
    {
        public UtilisateurRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Utilisateur?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Actif);
        }

        public async Task<Utilisateur?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Emprunts.OrderByDescending(e => e.DateEmprunt))
                    .ThenInclude(e => e.Livre)
                        .ThenInclude(l => l!.Auteur)
                .Include(u => u.Reservations.Where(r => r.Statut == "EnAttente" || r.Statut == "Disponible"))
                    .ThenInclude(r => r.Livre)
                .Include(u => u.Notifications.Where(n => !n.EstLue).OrderByDescending(n => n.DateCreation))
                .FirstOrDefaultAsync(u => u.IdUtilisateur == id);
        }

        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.IdUtilisateur != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Utilisateur>> GetAvecRetardsAsync()
        {
            return await _dbSet
                .Include(u => u.Emprunts.Where(e => e.Statut == "EnRetard" || (e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now)))
                    .ThenInclude(e => e.Livre)
                .Where(u => u.Actif && u.Emprunts.Any(e => e.Statut == "EnRetard" || (e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Utilisateur>> GetPlusActifsAsync(int nombre = 10)
        {
            return await _dbSet
                .Include(u => u.Emprunts)
                .Where(u => u.Actif)
                .OrderByDescending(u => u.Emprunts.Count)
                .Take(nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Utilisateur>> RechercherAsync(string terme)
        {
            var termeNormalise = terme.ToLower();
            return await _dbSet
                .Where(u => u.Actif &&
                    (u.Nom.ToLower().Contains(termeNormalise) ||
                     u.Prenom.ToLower().Contains(termeNormalise) ||
                     u.Email.ToLower().Contains(termeNormalise)))
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenom)
                .ToListAsync();
        }

        public async Task BloquerAsync(int id, string raison)
        {
            var utilisateur = await _dbSet.FindAsync(id);
            if (utilisateur != null)
            {
                utilisateur.EstBloque = true;
                utilisateur.RaisonBlocage = raison;
            }
        }

        public async Task DebloquerAsync(int id)
        {
            var utilisateur = await _dbSet.FindAsync(id);
            if (utilisateur != null)
            {
                utilisateur.EstBloque = false;
                utilisateur.RaisonBlocage = null;
            }
        }

        public async Task UpdateDerniereConnexionAsync(int id)
        {
            var utilisateur = await _dbSet.FindAsync(id);
            if (utilisateur != null)
            {
                utilisateur.DerniereConnexion = DateTime.Now;
            }
        }
    }
}
