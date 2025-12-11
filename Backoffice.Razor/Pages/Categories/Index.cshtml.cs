using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Categories
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<CategorieViewModel> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var livres = await _unitOfWork.Livres.GetAllAsync();

            Categories = categories
                .Where(c => c.Actif)
                .OrderBy(c => c.Nom)
                .Select(c => new CategorieViewModel
                {
                    IdCategorie = c.IdCategorie,
                    Nom = c.Nom,
                    Description = c.Description,
                    Couleur = c.Couleur ?? "#6c757d",
                    NombreLivres = c.LivreCategories?.Count ?? 0
                })
                .ToList();
        }

        public class CategorieViewModel
        {
            public int IdCategorie { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Couleur { get; set; } = "#6c757d";
            public int NombreLivres { get; set; }
        }
    }
}
