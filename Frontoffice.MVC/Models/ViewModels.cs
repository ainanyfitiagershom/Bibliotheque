using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Frontoffice.MVC.Models
{
    public class HomeViewModel
    {
        public List<LivreDTO> Nouveautes { get; set; } = new();
        public List<LivreDTO> Populaires { get; set; } = new();
        public List<LivreDTO> Recommandations { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();
    }

    public class LivresIndexViewModel
    {
        public PagedResultDTO<LivreDTO> Livres { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();
        public string? Search { get; set; }
        public int? CategorieId { get; set; }
        public string Tri { get; set; } = "titre";
    }

    public class LivreDetailsViewModel
    {
        public LivreDTO Livre { get; set; } = new();
        public int PositionFileAttente { get; set; }
        public List<Avis> Avis { get; set; } = new();
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CompteIndexViewModel
    {
        public Utilisateur Utilisateur { get; set; } = new();
        public List<EmpruntDTO> EmpruntsEnCours { get; set; } = new();
        public List<ReservationDTO> Reservations { get; set; } = new();
        public int NombreNotificationsNonLues { get; set; }
    }

    public class ProfilViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        public string Prenom { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        public string? NumeroAbonne { get; set; }
        public DateTime DateInscription { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string AncienMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez confirmer le mot de passe")]
        [Compare("NouveauMotDePasse", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmerMotDePasse { get; set; } = string.Empty;
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
