using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Backoffice.Razor.Pages.Livres
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public EditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public LivreEditInput Input { get; set; } = new();

        public Livre? Livre { get; set; }
        public List<Auteur> Auteurs { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Livre = await _unitOfWork.Livres.GetByIdWithDetailsAsync(id);
            if (Livre == null) return NotFound();

            Input = new LivreEditInput
            {
                IdLivre = Livre.IdLivre,
                ISBN = Livre.ISBN,
                Titre = Livre.Titre,
                IdAuteur = Livre.IdAuteur,
                Annee = Livre.Annee,
                Editeur = Livre.Editeur,
                NombrePages = Livre.NombrePages,
                Langue = Livre.Langue,
                Description = Livre.Description,
                ImageCouverture = Livre.ImageCouverture,
                Stock = Livre.Stock,
                Emplacement = Livre.Emplacement,
                CategorieIds = Livre.LivreCategories.Select(lc => lc.IdCategorie).ToList()
            };

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var livre = await _unitOfWork.Livres.GetByIdAsync(Input.IdLivre);
            if (livre == null) return NotFound();

            // Vérifier ISBN
            if (!string.IsNullOrEmpty(Input.ISBN) &&
                await _unitOfWork.Livres.IsbnExisteAsync(Input.ISBN, Input.IdLivre))
            {
                ModelState.AddModelError("Input.ISBN", "Cet ISBN existe déjà.");
                await LoadDataAsync();
                return Page();
            }

            // Mettre à jour
            livre.ISBN = Input.ISBN;
            livre.Titre = Input.Titre;
            livre.IdAuteur = Input.IdAuteur;
            livre.Annee = Input.Annee;
            livre.Editeur = Input.Editeur;
            livre.NombrePages = Input.NombrePages;
            livre.Langue = Input.Langue;
            livre.Description = Input.Description;
            livre.ImageCouverture = Input.ImageCouverture;
            livre.Emplacement = Input.Emplacement;
            livre.DateModification = DateTime.Now;

            // Gérer le stock
            var diff = Input.Stock - livre.Stock;
            livre.Stock = Input.Stock;
            livre.StockDisponible = Math.Max(0, livre.StockDisponible + diff);

            await _unitOfWork.Livres.UpdateAsync(livre);
            await _unitOfWork.Livres.UpdateCategoriesAsync(livre.IdLivre, Input.CategorieIds);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Livre modifié avec succès.";
            return RedirectToPage("/Livres/Index");
        }

        private async Task LoadDataAsync()
        {
            var auteurs = await _unitOfWork.Auteurs.GetAllAsync();
            Auteurs = auteurs.Where(a => a.Actif).OrderBy(a => a.Nom).ToList();

            var categories = await _unitOfWork.Categories.GetAllAsync();
            Categories = categories.Where(c => c.Actif).OrderBy(c => c.Nom).ToList();
        }

        public class LivreEditInput
        {
            public int IdLivre { get; set; }
            public string? ISBN { get; set; }
            [Required] public string Titre { get; set; } = "";
            [Required] public int IdAuteur { get; set; }
            public int? Annee { get; set; }
            public string? Editeur { get; set; }
            public int? NombrePages { get; set; }
            public string Langue { get; set; } = "Français";
            public string? Description { get; set; }
            public string? ImageCouverture { get; set; }
            public int Stock { get; set; } = 1;
            public string? Emplacement { get; set; }
            public List<int> CategorieIds { get; set; } = new();
        }
    }
}
