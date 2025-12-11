using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Livres
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Livre? Livre { get; set; }
        public int EmpruntsEnCours { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Livre = await _unitOfWork.Livres.GetByIdWithDetailsAsync(id);
            if (Livre == null) return NotFound();

            EmpruntsEnCours = await _unitOfWork.Emprunts.CountAsync(e =>
                e.IdLivre == id && (e.Statut == "EnCours" || e.Statut == "EnRetard"));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var livre = await _unitOfWork.Livres.GetByIdAsync(id);
            if (livre == null) return NotFound();

            // Vérifier les emprunts en cours
            var empruntsEnCours = await _unitOfWork.Emprunts.CountAsync(e =>
                e.IdLivre == id && (e.Statut == "EnCours" || e.Statut == "EnRetard"));

            if (empruntsEnCours > 0)
            {
                TempData["Error"] = "Impossible de supprimer : des emprunts sont en cours.";
                return RedirectToPage();
            }

            // Suppression logique
            livre.Actif = false;
            await _unitOfWork.Livres.UpdateAsync(livre);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Livre supprimé avec succès.";
            return RedirectToPage("/Livres/Index");
        }
    }
}
