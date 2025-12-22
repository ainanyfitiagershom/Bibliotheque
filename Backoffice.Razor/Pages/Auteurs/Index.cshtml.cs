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
        private const int PageSize = 15;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Auteur> Auteurs { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Recherche { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

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

            var query = auteurs.Where(a => a.Actif).OrderByDescending(a => a.IdAuteur);
            TotalItems = query.Count();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            Auteurs = query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
