using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Categories
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Categorie? Categorie { get; set; }
        public List<Livre> Livres { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Categorie = await _unitOfWork.Categories.GetByIdAsync(id);
            if (Categorie == null) return NotFound();

            // Récupérer les livres de cette catégorie
            var livres = await _unitOfWork.Livres.GetByCategorieAsync(id);
            Livres = livres.ToList();

            return Page();
        }
    }
}
