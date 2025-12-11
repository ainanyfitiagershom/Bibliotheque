using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
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
        public UtilisateurEditDTO Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(id);
            if (user == null) return NotFound();

            Input = new UtilisateurEditDTO
            {
                IdUtilisateur = user.IdUtilisateur,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Adresse = user.Adresse,
                DateNaissance = user.DateNaissance,
                Actif = user.Actif,
                EstBloque = user.EstBloque
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? NouveauMotDePasse)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(Input.IdUtilisateur);
            if (user == null) return NotFound();

            // Vérifier si l'email existe déjà pour un autre utilisateur
            var allUsers = await _unitOfWork.Utilisateurs.GetAllAsync();
            if (allUsers.Any(u => u.IdUtilisateur != Input.IdUtilisateur &&
                                  u.Email.Equals(Input.Email, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Input.Email", "Cet email est déjà utilisé.");
                return Page();
            }

            user.Nom = Input.Nom;
            user.Prenom = Input.Prenom;
            user.Email = Input.Email;
            user.Telephone = Input.Telephone;
            user.Adresse = Input.Adresse;
            user.DateNaissance = Input.DateNaissance;
            user.Actif = Input.Actif;
            user.EstBloque = Input.EstBloque;

            // Changer le mot de passe si fourni
            if (!string.IsNullOrEmpty(NouveauMotDePasse))
            {
                if (NouveauMotDePasse.Length < 6)
                {
                    ModelState.AddModelError("", "Le mot de passe doit contenir au moins 6 caractères.");
                    return Page();
                }
                user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(NouveauMotDePasse);
            }

            await _unitOfWork.Utilisateurs.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Utilisateur modifié avec succès.";
            return RedirectToPage("/Utilisateurs/Index");
        }
    }
}
