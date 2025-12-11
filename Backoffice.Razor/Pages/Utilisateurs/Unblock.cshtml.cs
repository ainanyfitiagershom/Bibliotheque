using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
{
    [Authorize]
    public class UnblockModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnblockModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.EstBloque = false;
            await _unitOfWork.Utilisateurs.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Utilisateur débloqué avec succès.";
            return RedirectToPage("/Utilisateurs/Index");
        }
    }
}
