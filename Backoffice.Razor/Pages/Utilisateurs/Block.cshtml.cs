using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
{
    [Authorize]
    public class BlockModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlockModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Utilisateur? Utilisateur { get; set; }
        public int EmpruntsEnCours { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(id);
            if (Utilisateur == null) return NotFound();

            EmpruntsEnCours = await _unitOfWork.Emprunts.CountAsync(e =>
                e.IdUtilisateur == id && (e.Statut == "EnCours" || e.Statut == "EnRetard"));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.EstBloque = true;
            await _unitOfWork.Utilisateurs.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Utilisateur bloqué avec succès.";
            return RedirectToPage("/Utilisateurs/Index");
        }
    }
}
