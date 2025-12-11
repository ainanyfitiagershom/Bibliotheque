using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class EmpruntRepository : Repository<Emprunt>, IEmpruntRepository
    {
        public EmpruntRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Emprunt?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Include(e => e.Utilisateur)
                .FirstOrDefaultAsync(e => e.IdEmprunt == id);
        }

        public async Task<IEnumerable<Emprunt>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(e => e.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Include(e => e.Utilisateur)
                .OrderByDescending(e => e.DateEmprunt)
                .ToListAsync();
        }

        public async Task<PagedResultDTO<EmpruntDTO>> GetFiltreAsync(EmpruntFiltreDTO filtre)
        {
            var query = _dbSet
                .Include(e => e.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Include(e => e.Utilisateur)
                .AsQueryable();

            // Filtres
            if (filtre.IdUtilisateur.HasValue)
            {
                query = query.Where(e => e.IdUtilisateur == filtre.IdUtilisateur);
            }

            if (filtre.IdLivre.HasValue)
            {
                query = query.Where(e => e.IdLivre == filtre.IdLivre);
            }

            if (!string.IsNullOrEmpty(filtre.Statut) && filtre.Statut != "Tous")
            {
                query = query.Where(e => e.Statut == filtre.Statut);
            }

            if (filtre.DateDebut.HasValue)
            {
                query = query.Where(e => e.DateEmprunt >= filtre.DateDebut);
            }

            if (filtre.DateFin.HasValue)
            {
                query = query.Where(e => e.DateEmprunt <= filtre.DateFin);
            }

            if (filtre.EnRetard.HasValue && filtre.EnRetard.Value)
            {
                query = query.Where(e => e.Statut == "EnRetard" ||
                    (e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.DateEmprunt)
                .Skip((filtre.Page - 1) * filtre.TaillePage)
                .Take(filtre.TaillePage)
                .Select(e => new EmpruntDTO
                {
                    IdEmprunt = e.IdEmprunt,
                    IdLivre = e.IdLivre,
                    LivreTitre = e.Livre != null ? e.Livre.Titre : "",
                    LivreImage = e.Livre != null ? e.Livre.ImageCouverture : null,
                    IdUtilisateur = e.IdUtilisateur,
                    UtilisateurNom = e.Utilisateur != null ? e.Utilisateur.Prenom + " " + e.Utilisateur.Nom : "",
                    UtilisateurEmail = e.Utilisateur != null ? e.Utilisateur.Email : "",
                    DateEmprunt = e.DateEmprunt,
                    DateRetourPrevue = e.DateRetourPrevue,
                    DateRetourEffective = e.DateRetourEffective,
                    Statut = e.Statut,
                    NombreProlongations = e.NombreProlongations,
                    MaxProlongations = e.MaxProlongations,
                    Penalite = e.Penalite,
                    Notes = e.Notes
                })
                .ToListAsync();

            return new PagedResultDTO<EmpruntDTO>
            {
                Items = items,
                TotalItems = totalCount,
                Page = filtre.Page,
                TaillePage = filtre.TaillePage
            };
        }

        public async Task<IEnumerable<Emprunt>> GetByUtilisateurAsync(int idUtilisateur, bool enCoursUniquement = false)
        {
            var query = _dbSet
                .Include(e => e.Livre)
                    .ThenInclude(l => l!.Auteur)
                .Where(e => e.IdUtilisateur == idUtilisateur);

            if (enCoursUniquement)
            {
                query = query.Where(e => e.Statut == "EnCours" || e.Statut == "EnRetard");
            }

            return await query
                .OrderByDescending(e => e.DateEmprunt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Emprunt>> GetByLivreAsync(int idLivre)
        {
            return await _dbSet
                .Include(e => e.Utilisateur)
                .Where(e => e.IdLivre == idLivre)
                .OrderByDescending(e => e.DateEmprunt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Emprunt>> GetEnRetardAsync()
        {
            return await _dbSet
                .Include(e => e.Livre)
                .Include(e => e.Utilisateur)
                .Where(e => e.Statut == "EnRetard" ||
                    (e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now))
                .OrderBy(e => e.DateRetourPrevue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Emprunt>> GetProchesEcheanceAsync(int jours = 2)
        {
            var dateLimite = DateTime.Now.AddDays(jours);
            return await _dbSet
                .Include(e => e.Livre)
                .Include(e => e.Utilisateur)
                .Where(e => e.Statut == "EnCours" &&
                    e.DateRetourPrevue <= dateLimite &&
                    e.DateRetourPrevue >= DateTime.Now)
                .OrderBy(e => e.DateRetourPrevue)
                .ToListAsync();
        }

        public async Task<bool> ADejaEmprunteAsync(int idUtilisateur, int idLivre)
        {
            return await _dbSet.AnyAsync(e =>
                e.IdUtilisateur == idUtilisateur &&
                e.IdLivre == idLivre &&
                (e.Statut == "EnCours" || e.Statut == "EnRetard"));
        }

        public async Task<int> CompterEmpruntsEnCoursAsync(int idUtilisateur)
        {
            return await _dbSet.CountAsync(e =>
                e.IdUtilisateur == idUtilisateur &&
                (e.Statut == "EnCours" || e.Statut == "EnRetard"));
        }

        public async Task<IEnumerable<EmpruntParMoisDTO>> GetStatistiquesParMoisAsync(int nombreMois = 12)
        {
            var dateDebut = DateTime.Now.AddMonths(-nombreMois);

            return await _dbSet
                .Where(e => e.DateEmprunt >= dateDebut)
                .GroupBy(e => new { e.DateEmprunt.Year, e.DateEmprunt.Month })
                .Select(g => new EmpruntParMoisDTO
                {
                    Mois = $"{g.Key.Year}-{g.Key.Month:D2}",
                    NombreEmprunts = g.Count()
                })
                .OrderBy(x => x.Mois)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmpruntParCategorieDTO>> GetStatistiquesParCategorieAsync()
        {
            return await _context.Emprunts
                .Join(_context.LivreCategories,
                    e => e.IdLivre,
                    lc => lc.IdLivre,
                    (e, lc) => new { Emprunt = e, LivreCategorie = lc })
                .Join(_context.Categories,
                    x => x.LivreCategorie.IdCategorie,
                    c => c.IdCategorie,
                    (x, c) => new { x.Emprunt, Categorie = c })
                .GroupBy(x => new { x.Categorie.Nom, x.Categorie.Couleur })
                .Select(g => new EmpruntParCategorieDTO
                {
                    Categorie = g.Key.Nom,
                    NombreEmprunts = g.Count(),
                    Couleur = g.Key.Couleur
                })
                .OrderByDescending(x => x.NombreEmprunts)
                .ToListAsync();
        }
    }
}
