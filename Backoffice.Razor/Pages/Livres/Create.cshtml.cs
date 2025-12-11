using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Livres
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
        public LivreCreateDTO Input { get; set; } = new();

        public List<Auteur> Auteurs { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Vérifier si l'ISBN existe déjà
            if (!string.IsNullOrEmpty(Input.ISBN) && await _unitOfWork.Livres.IsbnExisteAsync(Input.ISBN))
            {
                ModelState.AddModelError("Input.ISBN", "Cet ISBN existe déjà.");
                await LoadDataAsync();
                return Page();
            }

            // Créer le livre
            var livre = new Livre
            {
                ISBN = Input.ISBN,
                Titre = Input.Titre,
                IdAuteur = Input.IdAuteur,
                Annee = Input.Annee,
                Editeur = Input.Editeur,
                NombrePages = Input.NombrePages,
                Langue = Input.Langue,
                Description = Input.Description,
                ImageCouverture = Input.ImageCouverture,
                Stock = Input.Stock,
                StockDisponible = Input.Stock,
                Emplacement = Input.Emplacement,
                DateAjout = DateTime.Now,
                Actif = true
            };

            await _unitOfWork.Livres.AddAsync(livre);
            await _unitOfWork.SaveChangesAsync();

            // Ajouter les catégories
            if (Input.CategorieIds.Any())
            {
                await _unitOfWork.Livres.UpdateCategoriesAsync(livre.IdLivre, Input.CategorieIds);
                await _unitOfWork.SaveChangesAsync();
            }

            TempData["Success"] = "Livre ajouté avec succès.";
            return RedirectToPage("/Livres/Index");
        }

        private async Task LoadDataAsync()
        {
            var auteurs = await _unitOfWork.Auteurs.GetAllAsync();
            Auteurs = auteurs.Where(a => a.Actif).OrderBy(a => a.Nom).ToList();

            var categories = await _unitOfWork.Categories.GetAllAsync();
            Categories = categories.Where(c => c.Actif).OrderBy(c => c.Nom).ToList();
        }
    }
}
