using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Emprunts
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Emprunt? Emprunt { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(id);
            if (Emprunt == null) return NotFound();

            return Page();
        }
    }
}
