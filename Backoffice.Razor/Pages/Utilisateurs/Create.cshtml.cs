using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
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
        public UtilisateurCreateDTO Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Vérifier si l'email existe déjà
            if (await _unitOfWork.Utilisateurs.EmailExisteAsync(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Cet email est déjà utilisé.");
                return Page();
            }

            var utilisateur = new Utilisateur
            {
                Nom = Input.Nom,
                Prenom = Input.Prenom,
                Email = Input.Email,
                Telephone = Input.Telephone,
                Adresse = Input.Adresse,
                DateNaissance = Input.DateNaissance,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(Input.MotDePasse),
                DateInscription = DateTime.Now,
                Actif = true,
                EstBloque = false
            };

            await _unitOfWork.Utilisateurs.AddAsync(utilisateur);
            await _unitOfWork.SaveChangesAsync();

            // Créer une notification de bienvenue
            await _unitOfWork.Notifications.CreerNotificationBienvenueAsync(utilisateur.IdUtilisateur);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Utilisateur créé avec succès. Numéro d'abonné : {utilisateur.NumeroAbonne}";
            return RedirectToPage("/Utilisateurs/Index");
        }
    }
}
