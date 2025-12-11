using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Emprunts
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public int IdUtilisateur { get; set; }

        [BindProperty]
        public int IdLivre { get; set; }

        [BindProperty]
        public DateTime DateEmprunt { get; set; } = DateTime.Today;

        [BindProperty]
        public DateTime DateRetourPrevue { get; set; } = DateTime.Today.AddDays(21);

        public List<Utilisateur> Utilisateurs { get; set; } = new();
        public List<Livre> Livres { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Vérifications
            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(IdUtilisateur);
            if (user == null)
            {
                ModelState.AddModelError("", "Utilisateur non trouvé.");
                await LoadDataAsync();
                return Page();
            }

            if (user.Statut == "Bloque")
            {
                ModelState.AddModelError("", "Cet utilisateur est bloqué et ne peut pas emprunter.");
                await LoadDataAsync();
                return Page();
            }

            var livre = await _unitOfWork.Livres.GetByIdAsync(IdLivre);
            if (livre == null)
            {
                ModelState.AddModelError("", "Livre non trouvé.");
                await LoadDataAsync();
                return Page();
            }

            if (livre.StockDisponible <= 0)
            {
                ModelState.AddModelError("", "Ce livre n'est plus disponible.");
                await LoadDataAsync();
                return Page();
            }

            // Vérifier le nombre d'emprunts en cours
            var empruntsEnCours = await _unitOfWork.Emprunts.CountAsync(e =>
                e.IdUtilisateur == IdUtilisateur && (e.Statut == "EnCours" || e.Statut == "EnRetard"));

            if (empruntsEnCours >= 3)
            {
                ModelState.AddModelError("", "Cet utilisateur a déjà 3 emprunts en cours (maximum autorisé).");
                await LoadDataAsync();
                return Page();
            }

            // Vérifier la durée
            if ((DateRetourPrevue - DateEmprunt).TotalDays > 21)
            {
                ModelState.AddModelError("", "La durée d'emprunt ne peut pas dépasser 21 jours.");
                await LoadDataAsync();
                return Page();
            }

            // Créer l'emprunt
            var emprunt = new Emprunt
            {
                IdUtilisateur = IdUtilisateur,
                IdLivre = IdLivre,
                DateEmprunt = DateEmprunt,
                DateRetourPrevue = DateRetourPrevue,
                Statut = "EnCours",
                NombreProlongations = 0
            };

            await _unitOfWork.Emprunts.AddAsync(emprunt);

            // Mettre à jour le stock
            livre.StockDisponible--;
            await _unitOfWork.Livres.UpdateAsync(livre);

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Emprunt enregistré avec succès.";
            return RedirectToPage("/Emprunts/Index");
        }

        private async Task LoadDataAsync()
        {
            var users = await _unitOfWork.Utilisateurs.GetAllAsync();
            Utilisateurs = users.Where(u => u.Statut == "Actif").OrderBy(u => u.Nom).ToList();

            var livres = await _unitOfWork.Livres.GetAllAsync();
            Livres = livres.Where(l => l.Actif && l.StockDisponible > 0).OrderBy(l => l.Titre).ToList();
        }
    }
}
