using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Categories
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public EditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public CategorieDTO Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categorie = await _unitOfWork.Categories.GetByIdAsync(id);
            if (categorie == null) return NotFound();

            Input = new CategorieDTO
            {
                IdCategorie = categorie.IdCategorie,
                Nom = categorie.Nom,
                Description = categorie.Description,
                Couleur = categorie.Couleur ?? "#6c757d"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var categorie = await _unitOfWork.Categories.GetByIdAsync(Input.IdCategorie);
            if (categorie == null) return NotFound();

            // Vérifier si le nom existe déjà (sauf pour cette catégorie)
            var existing = await _unitOfWork.Categories.GetAllAsync();
            if (existing.Any(c => c.IdCategorie != Input.IdCategorie &&
                                  c.Nom.Equals(Input.Nom, StringComparison.OrdinalIgnoreCase) && c.Actif))
            {
                ModelState.AddModelError("Input.Nom", "Une catégorie avec ce nom existe déjà.");
                return Page();
            }

            categorie.Nom = Input.Nom;
            categorie.Description = Input.Description;
            categorie.Couleur = Input.Couleur;

            await _unitOfWork.Categories.UpdateAsync(categorie);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Catégorie modifiée avec succès.";
            return RedirectToPage("/Categories/Index");
        }
    }
}
