using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Auteurs
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Auteur? Auteur { get; set; }
        public int NombreLivres { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Auteur = await _unitOfWork.Auteurs.GetByIdAsync(id);
            if (Auteur == null) return NotFound();

            // Compter les livres de cet auteur
            NombreLivres = await _unitOfWork.Livres.CountAsync(l => l.IdAuteur == id && l.Actif);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var auteur = await _unitOfWork.Auteurs.GetByIdAsync(id);
            if (auteur == null) return NotFound();

            // Suppression logique
            auteur.Actif = false;
            await _unitOfWork.Auteurs.UpdateAsync(auteur);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Auteur supprimé avec succès.";
            return RedirectToPage("/Auteurs/Index");
        }
    }
}
