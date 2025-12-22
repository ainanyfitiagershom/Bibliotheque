using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
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

        public List<UtilisateurViewModel> Utilisateurs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Statut { get; set; }

        public async Task OnGetAsync()
        {

            var allUsers = await _unitOfWork.Utilisateurs.GetAllAsync();
            var emprunts = await _unitOfWork.Emprunts.GetAllAsync();

            var query = allUsers.AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                var searchLower = Search.ToLower();
                query = query.Where(u =>
                    u.Nom.ToLower().Contains(searchLower) ||
                    u.Prenom.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.Telephone != null && u.Telephone.Contains(searchLower)) ||
                    (u.NumeroAbonne != null && u.NumeroAbonne.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(Statut))
            {
                query = query.Where(u => u.Statut == Statut);
            }

            var total = query.Count();
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            Utilisateurs = query
                .OrderByDescending(u => u.IdUtilisateur)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UtilisateurViewModel
                {
                    IdUtilisateur = u.IdUtilisateur,
                    Nom = u.Nom,
                    Prenom = u.Prenom,
                    Email = u.Email,
                    Telephone = u.Telephone,
                    NumeroAbonne = u.NumeroAbonne ?? $"USR{u.IdUtilisateur:D5}",
                    DateInscription = u.DateInscription,
                    Statut = u.Statut ?? "Actif",
                    EmpruntsEnCours = emprunts.Count(e => e.IdUtilisateur == u.IdUtilisateur && e.Statut == "EnCours"),
                    EmpruntsEnRetard = emprunts.Count(e => e.IdUtilisateur == u.IdUtilisateur && e.Statut == "EnRetard")
                })
                .ToList();
        }

        public class UtilisateurViewModel
        {
            public int IdUtilisateur { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Prenom { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Telephone { get; set; }
            public string NumeroAbonne { get; set; } = string.Empty;
            public DateTime DateInscription { get; set; }
            public string Statut { get; set; } = "Actif";
            public int EmpruntsEnCours { get; set; }
            public int EmpruntsEnRetard { get; set; }
        }
    }
}
