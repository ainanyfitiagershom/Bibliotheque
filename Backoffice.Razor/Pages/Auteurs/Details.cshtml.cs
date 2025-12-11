using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Auteurs
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Auteur? Auteur { get; set; }
        public List<Livre> Livres { get; set; } = new();
        public int NombreLivres { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Auteur = await _unitOfWork.Auteurs.GetByIdAsync(id);
            if (Auteur == null) return NotFound();

            // Récupérer les livres de cet auteur
            var allLivres = await _unitOfWork.Livres.GetAllAsync();
            Livres = allLivres.Where(l => l.IdAuteur == id && l.Actif).ToList();
            NombreLivres = Livres.Count;

            return Page();
        }
    }
}
