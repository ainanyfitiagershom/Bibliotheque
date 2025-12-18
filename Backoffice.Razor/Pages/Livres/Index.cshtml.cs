using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Livres
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<LivreDTO> Livres { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Recherche { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? IdCategorie { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? Disponible { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Tri { get; set; } = "Recent";

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        public async Task OnGetAsync()
        {
            // Charger les catégories pour le filtre
            var categories = await _unitOfWork.Categories.GetAllAsync();
            Categories = categories.Where(c => c.Actif).OrderBy(c => c.Nom).ToList();

            // Rechercher les livres
            var recherche = new LivreRechercheDTO
            {
                Recherche = Recherche,
                IdCategorie = IdCategorie,
                Disponible = Disponible,
                Tri = Tri,
                Page = Page,
                TaillePage = 15
            };

            var result = await _unitOfWork.Livres.RechercherAsync(recherche);
            Livres = result.Items;
            TotalItems = result.TotalItems;
            TotalPages = result.TotalPages;
        }
    }
}
