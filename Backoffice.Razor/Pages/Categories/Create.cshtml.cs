using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Categories
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public CategorieDTO Input { get; set; } = new() { Couleur = "#6c757d" };

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Vérifier si le nom existe déjà
            var existing = await _unitOfWork.Categories.GetAllAsync();
            if (existing.Any(c => c.Nom.Equals(Input.Nom, StringComparison.OrdinalIgnoreCase) && c.Actif))
            {
                ModelState.AddModelError("Input.Nom", "Une catégorie avec ce nom existe déjà.");
                return Page();
            }

            var categorie = new Categorie
            {
                Nom = Input.Nom,
                Description = Input.Description,
                Couleur = Input.Couleur,
                Actif = true
            };

            await _unitOfWork.Categories.AddAsync(categorie);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Catégorie créée avec succès.";
            return RedirectToPage("/Categories/Index");
        }
    }
}
