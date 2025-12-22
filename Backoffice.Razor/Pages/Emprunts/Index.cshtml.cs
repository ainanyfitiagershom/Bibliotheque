using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Emprunts
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int PageSize = 20;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<EmpruntViewModel> Emprunts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Statut { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateDebut { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFin { get; set; }

        // Statistiques
        public int TotalEnCours { get; set; }
        public int TotalEnRetard { get; set; }
        public int TotalTermines { get; set; }
        public int TotalAujourdhui { get; set; }

        public async Task OnGetAsync()
        {

            var allEmprunts = await _unitOfWork.Emprunts.GetAllWithDetailsAsync();
            var livres = await _unitOfWork.Livres.GetAllAsync();
            var users = await _unitOfWork.Utilisateurs.GetAllAsync();

            // Statistiques
            TotalEnCours = allEmprunts.Count(e => e.Statut == "EnCours");
            TotalEnRetard = allEmprunts.Count(e => e.Statut == "EnRetard");
            TotalTermines = allEmprunts.Count(e => e.Statut == "Termine" &&
                e.DateRetourEffective?.Month == DateTime.Now.Month &&
                e.DateRetourEffective?.Year == DateTime.Now.Year);
            TotalAujourdhui = allEmprunts.Count(e => e.DateEmprunt.Date == DateTime.Today);

            var query = allEmprunts.AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                var searchLower = Search.ToLower();
                var livreIds = livres.Where(l => l.Titre.ToLower().Contains(searchLower)).Select(l => l.IdLivre);
                var userIds = users.Where(u => u.Nom.ToLower().Contains(searchLower) ||
                    u.Prenom.ToLower().Contains(searchLower)).Select(u => u.IdUtilisateur);

                query = query.Where(e => livreIds.Contains(e.IdLivre) || userIds.Contains(e.IdUtilisateur));
            }

            if (!string.IsNullOrEmpty(Statut))
            {
                query = query.Where(e => e.Statut == Statut);
            }

            if (DateDebut.HasValue)
            {
                query = query.Where(e => e.DateEmprunt.Date >= DateDebut.Value.Date);
            }

            if (DateFin.HasValue)
            {
                query = query.Where(e => e.DateEmprunt.Date <= DateFin.Value.Date);
            }

            var total = query.Count();
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            Emprunts = query
                .OrderByDescending(e => e.IdEmprunt)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(e => new EmpruntViewModel
                {
                    IdEmprunt = e.IdEmprunt,
                    IdLivre = e.IdLivre,
                    IdUtilisateur = e.IdUtilisateur,
                    TitreLivre = e.Livre != null ? e.Livre.Titre : "N/A",
                    NomUtilisateur = e.Utilisateur != null ? $"{e.Utilisateur.Nom} {e.Utilisateur.Prenom}" : "N/A",
                    DateEmprunt = e.DateEmprunt,
                    DateRetourPrevue = e.DateRetourPrevue,
                    DateRetourEffective = e.DateRetourEffective,
                    Statut = e.Statut ?? "EnCours",
                    NombreProlongations = e.NombreProlongations
                })
                .ToList();
        }

        public class EmpruntViewModel
        {
            public int IdEmprunt { get; set; }
            public int IdLivre { get; set; }
            public int IdUtilisateur { get; set; }
            public string TitreLivre { get; set; } = string.Empty;
            public string NomUtilisateur { get; set; } = string.Empty;
            public DateTime DateEmprunt { get; set; }
            public DateTime DateRetourPrevue { get; set; }
            public DateTime? DateRetourEffective { get; set; }
            public string Statut { get; set; } = "EnCours";
            public int NombreProlongations { get; set; }
        }
    }
}
