using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Livres
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Livre? Livre { get; set; }
        public List<Emprunt> EmpruntsEnCours { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Livre = await _unitOfWork.Livres.GetByIdWithDetailsAsync(id);
            if (Livre == null) return NotFound();

            var emprunts = await _unitOfWork.Emprunts.GetByLivreAsync(id);
            EmpruntsEnCours = emprunts.Where(e => e.Statut != "Termine").ToList();

            return Page();
        }
    }
}
