using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Auteurs
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Auteur> Auteurs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Recherche { get; set; }

        public async Task OnGetAsync()
        {
            IEnumerable<Auteur> auteurs;

            if (!string.IsNullOrEmpty(Recherche))
            {
                auteurs = await _unitOfWork.Auteurs.RechercherAsync(Recherche);
            }
            else
            {
                auteurs = await _unitOfWork.Auteurs.GetAllAsync();
            }

            Auteurs = auteurs.Where(a => a.Actif).OrderBy(a => a.Nom).ToList();
        }
    }
}
