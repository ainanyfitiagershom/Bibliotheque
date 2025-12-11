using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Auteurs
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
        public AuteurDTO Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var auteur = await _unitOfWork.Auteurs.GetByIdAsync(id);
            if (auteur == null) return NotFound();

            Input = new AuteurDTO
            {
                IdAuteur = auteur.IdAuteur,
                Nom = auteur.Nom,
                Prenom = auteur.Prenom,
                Nationalite = auteur.Nationalite,
                DateNaissance = auteur.DateNaissance,
                DateDeces = auteur.DateDeces,
                Biographie = auteur.Biographie,
                PhotoUrl = auteur.PhotoUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var auteur = await _unitOfWork.Auteurs.GetByIdAsync(Input.IdAuteur);
            if (auteur == null) return NotFound();

            auteur.Nom = Input.Nom;
            auteur.Prenom = Input.Prenom;
            auteur.Nationalite = Input.Nationalite;
            auteur.DateNaissance = Input.DateNaissance;
            auteur.DateDeces = Input.DateDeces;
            auteur.Biographie = Input.Biographie;
            auteur.PhotoUrl = Input.PhotoUrl;

            await _unitOfWork.Auteurs.UpdateAsync(auteur);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Auteur modifié avec succès.";
            return RedirectToPage("/Auteurs/Index");
        }
    }
}
