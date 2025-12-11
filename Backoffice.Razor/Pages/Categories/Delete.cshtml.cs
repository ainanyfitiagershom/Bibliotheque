using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Categories
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Categorie? Categorie { get; set; }
        public int NombreLivres { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Categorie = await _unitOfWork.Categories.GetByIdAsync(id);
            if (Categorie == null) return NotFound();

            // Compter les livres de cette catégorie
            var livres = await _unitOfWork.Livres.GetByCategorieAsync(id);
            NombreLivres = livres.Count();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var categorie = await _unitOfWork.Categories.GetByIdAsync(id);
            if (categorie == null) return NotFound();

            // Suppression logique
            categorie.Actif = false;
            await _unitOfWork.Categories.UpdateAsync(categorie);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Catégorie supprimée avec succès.";
            return RedirectToPage("/Categories/Index");
        }
    }
}
