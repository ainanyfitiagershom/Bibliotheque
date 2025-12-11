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
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public string? Search { get; set; }
        public string? Statut { get; set; }

        public async Task OnGetAsync(int page = 1, string? search = null, string? statut = null)
        {
            CurrentPage = page;
            Search = search;
            Statut = statut;

            var allUsers = await _unitOfWork.Utilisateurs.GetAllAsync();
            var emprunts = await _unitOfWork.Emprunts.GetAllAsync();

            var query = allUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Nom.ToLower().Contains(search) ||
                    u.Prenom.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.Telephone != null && u.Telephone.Contains(search)) ||
                    (u.NumeroAbonne != null && u.NumeroAbonne.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(u => u.Statut == statut);
            }

            var total = query.Count();
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            Utilisateurs = query
                .OrderByDescending(u => u.DateInscription)
                .Skip((page - 1) * PageSize)
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
