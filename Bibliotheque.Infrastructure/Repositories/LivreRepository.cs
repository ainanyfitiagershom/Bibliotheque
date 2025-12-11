using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Repositories
{
    public class LivreRepository : Repository<Livre>, ILivreRepository
    {
        public LivreRepository(BibliothequeDbContext context) : base(context)
        {
        }

        public async Task<Livre?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Include(l => l.Avis.Where(a => a.Approuve))
                    .ThenInclude(a => a.Utilisateur)
                .FirstOrDefaultAsync(l => l.IdLivre == id);
        }

        public async Task<IEnumerable<Livre>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Where(l => l.Actif)
                .OrderBy(l => l.Titre)
                .ToListAsync();
        }

        public async Task<PagedResultDTO<LivreDTO>> RechercherAsync(LivreRechercheDTO recherche)
        {
            var query = _dbSet
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Where(l => l.Actif)
                .AsQueryable();

            // Filtre par recherche textuelle
            if (!string.IsNullOrWhiteSpace(recherche.Recherche))
            {
                var terme = recherche.Recherche.ToLower();
                query = query.Where(l =>
                    l.Titre.ToLower().Contains(terme) ||
                    (l.Auteur != null && l.Auteur.Nom.ToLower().Contains(terme)) ||
                    (l.Auteur != null && l.Auteur.Prenom != null && l.Auteur.Prenom.ToLower().Contains(terme)) ||
                    (l.ISBN != null && l.ISBN.Contains(terme)) ||
                    (l.Description != null && l.Description.ToLower().Contains(terme)));
            }

            // Filtre par catégorie
            if (recherche.IdCategorie.HasValue)
            {
                query = query.Where(l => l.LivreCategories.Any(lc => lc.IdCategorie == recherche.IdCategorie));
            }

            // Filtre par auteur
            if (recherche.IdAuteur.HasValue)
            {
                query = query.Where(l => l.IdAuteur == recherche.IdAuteur);
            }

            // Filtre par année
            if (recherche.Annee.HasValue)
            {
                query = query.Where(l => l.Annee == recherche.Annee);
            }

            // Filtre par disponibilité
            if (recherche.Disponible.HasValue)
            {
                query = recherche.Disponible.Value
                    ? query.Where(l => l.StockDisponible > 0)
                    : query.Where(l => l.StockDisponible == 0);
            }

            // Comptage total
            var totalCount = await query.CountAsync();

            // Tri
            query = recherche.Tri switch
            {
                "Annee" => query.OrderByDescending(l => l.Annee),
                "Popularite" => query.OrderByDescending(l => l.NombreEmprunts),
                "Note" => query.OrderByDescending(l => l.NoteMoyenne),
                "Recent" => query.OrderByDescending(l => l.DateAjout),
                _ => query.OrderBy(l => l.Titre)
            };

            // Pagination
            var items = await query
                .Skip((recherche.Page - 1) * recherche.TaillePage)
                .Take(recherche.TaillePage)
                .Select(l => new LivreDTO
                {
                    IdLivre = l.IdLivre,
                    ISBN = l.ISBN,
                    Titre = l.Titre,
                    IdAuteur = l.IdAuteur,
                    AuteurNom = l.Auteur != null
                        ? (l.Auteur.Prenom != null ? l.Auteur.Prenom + " " + l.Auteur.Nom : l.Auteur.Nom)
                        : "",
                    Annee = l.Annee,
                    Editeur = l.Editeur,
                    NombrePages = l.NombrePages,
                    Langue = l.Langue,
                    Description = l.Description,
                    ImageCouverture = l.ImageCouverture,
                    Stock = l.Stock,
                    StockDisponible = l.StockDisponible,
                    Emplacement = l.Emplacement,
                    NombreEmprunts = l.NombreEmprunts,
                    NoteMoyenne = l.NoteMoyenne,
                    Categories = l.LivreCategories.Select(lc => new CategorieSimpleDTO
                    {
                        IdCategorie = lc.Categorie!.IdCategorie,
                        Nom = lc.Categorie.Nom,
                        Couleur = lc.Categorie.Couleur
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResultDTO<LivreDTO>
            {
                Items = items,
                TotalItems = totalCount,
                Page = recherche.Page,
                TaillePage = recherche.TaillePage
            };
        }

        public async Task<IEnumerable<Livre>> GetByCategorieAsync(int idCategorie)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Where(l => l.Actif && l.LivreCategories.Any(lc => lc.IdCategorie == idCategorie))
                .OrderBy(l => l.Titre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Livre>> GetByAuteurAsync(int idAuteur)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Where(l => l.Actif && l.IdAuteur == idAuteur)
                .OrderBy(l => l.Titre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Livre>> GetDisponiblesAsync()
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Where(l => l.Actif && l.StockDisponible > 0)
                .OrderBy(l => l.Titre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Livre>> GetNouveautesAsync(int nombre = 10)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Where(l => l.Actif)
                .OrderByDescending(l => l.DateAjout)
                .Take(nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Livre>> GetPopulairesAsync(int nombre = 10)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Where(l => l.Actif)
                .OrderByDescending(l => l.NombreEmprunts)
                .Take(nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Livre>> GetMieuxNotesAsync(int nombre = 10)
        {
            return await _dbSet
                .Include(l => l.Auteur)
                .Where(l => l.Actif && l.NoteMoyenne > 0)
                .OrderByDescending(l => l.NoteMoyenne)
                .ThenByDescending(l => l.NombreEmprunts)
                .Take(nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<LivreDTO>> RechercheRapideAsync(string terme, int limite = 5)
        {
            var termeNormalise = terme.ToLower();

            return await _dbSet
                .Include(l => l.Auteur)
                .Where(l => l.Actif &&
                    (l.Titre.ToLower().Contains(termeNormalise) ||
                     (l.Auteur != null && l.Auteur.Nom.ToLower().Contains(termeNormalise)) ||
                     (l.ISBN != null && l.ISBN.Contains(terme))))
                .Take(limite)
                .Select(l => new LivreDTO
                {
                    IdLivre = l.IdLivre,
                    Titre = l.Titre,
                    AuteurNom = l.Auteur != null
                        ? (l.Auteur.Prenom != null ? l.Auteur.Prenom + " " + l.Auteur.Nom : l.Auteur.Nom)
                        : "",
                    ImageCouverture = l.ImageCouverture,
                    StockDisponible = l.StockDisponible
                })
                .ToListAsync();
        }

        public async Task<bool> IsbnExisteAsync(string isbn, int? excludeId = null)
        {
            var query = _dbSet.Where(l => l.ISBN == isbn);
            if (excludeId.HasValue)
            {
                query = query.Where(l => l.IdLivre != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task UpdateCategoriesAsync(int idLivre, List<int> categorieIds)
        {
            var livre = await _dbSet
                .Include(l => l.LivreCategories)
                .FirstOrDefaultAsync(l => l.IdLivre == idLivre);

            if (livre == null) return;

            // Supprimer les anciennes associations
            _context.LivreCategories.RemoveRange(livre.LivreCategories);

            // Ajouter les nouvelles associations
            foreach (var categorieId in categorieIds)
            {
                _context.LivreCategories.Add(new LivreCategorie
                {
                    IdLivre = idLivre,
                    IdCategorie = categorieId
                });
            }
        }

        public async Task UpdateNoteMoyenneAsync(int idLivre)
        {
            var livre = await _dbSet.FindAsync(idLivre);
            if (livre == null) return;

            var moyenne = await _context.Avis
                .Where(a => a.IdLivre == idLivre && a.Approuve)
                .AverageAsync(a => (decimal?)a.Note) ?? 0;

            livre.NoteMoyenne = Math.Round(moyenne, 2);
        }
    }
}
