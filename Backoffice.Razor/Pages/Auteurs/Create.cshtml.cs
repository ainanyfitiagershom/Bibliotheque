using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Backoffice.Razor.Pages.Auteurs
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
        public AuteurInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var auteur = new Auteur
            {
                Nom = Input.Nom,
                Prenom = Input.Prenom,
                Nationalite = Input.Nationalite,
                DateNaissance = Input.DateNaissance,
                DateDeces = Input.DateDeces,
                PhotoUrl = Input.PhotoUrl,
                Biographie = Input.Biographie,
                DateCreation = DateTime.Now,
                Actif = true
            };

            await _unitOfWork.Auteurs.AddAsync(auteur);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Auteur ajouté avec succès.";
            return RedirectToPage("/Auteurs/Index");
        }

        public class AuteurInput
        {
            [Required(ErrorMessage = "Le nom est obligatoire")]
            [Display(Name = "Nom")]
            public string Nom { get; set; } = "";

            [Display(Name = "Prénom")]
            public string? Prenom { get; set; }

            [Display(Name = "Nationalité")]
            public string? Nationalite { get; set; }

            [Display(Name = "Date de naissance")]
            public DateTime? DateNaissance { get; set; }

            [Display(Name = "Date de décès")]
            public DateTime? DateDeces { get; set; }

            [Display(Name = "Photo (URL)")]
            public string? PhotoUrl { get; set; }

            [Display(Name = "Biographie")]
            public string? Biographie { get; set; }
        }
    }
}
