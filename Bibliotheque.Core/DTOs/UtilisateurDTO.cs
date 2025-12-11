using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour afficher un utilisateur
    /// </summary>
    public class UtilisateurDTO
    {
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public DateTime DateInscription { get; set; }
        public DateTime? DerniereConnexion { get; set; }
        public int NombreEmpruntsMax { get; set; }
        public bool Actif { get; set; }
        public bool EstBloque { get; set; }
        public string? RaisonBlocage { get; set; }

        // Propriétés calculées
        public string NomComplet => $"{Prenom} {Nom}";
        public string NumeroAbonne => $"AB{IdUtilisateur:D6}";
        public string Statut => !Actif ? "Inactif" : EstBloque ? "Bloqué" : "Actif";
    }

    /// <summary>
    /// DTO pour créer un utilisateur
    /// </summary>
    public class UtilisateurCreateDTO
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir entre 6 et 100 caractères")]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20)]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [StringLength(500)]
        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        [Display(Name = "Date de naissance")]
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        [Range(1, 10, ErrorMessage = "Le nombre d'emprunts max doit être entre 1 et 10")]
        [Display(Name = "Emprunts maximum")]
        public int NombreEmpruntsMax { get; set; } = 3;
    }

    /// <summary>
    /// DTO pour modifier un utilisateur
    /// </summary>
    public class UtilisateurEditDTO
    {
        public int IdUtilisateur { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20)]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [StringLength(500)]
        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        [Display(Name = "Date de naissance")]
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        [Range(1, 10, ErrorMessage = "Le nombre d'emprunts max doit être entre 1 et 10")]
        [Display(Name = "Emprunts maximum")]
        public int NombreEmpruntsMax { get; set; } = 3;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        [Display(Name = "Bloqué")]
        public bool EstBloque { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Raison du blocage")]
        public string? RaisonBlocage { get; set; }

        // Propriété calculée pour compatibilité
        public string Statut => !Actif ? "Inactif" : EstBloque ? "Bloqué" : "Actif";
    }

    /// <summary>
    /// DTO pour changer le mot de passe
    /// </summary>
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "L'ancien mot de passe est obligatoire")]
        [Display(Name = "Ancien mot de passe")]
        public string AncienMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir entre 6 et 100 caractères")]
        [Display(Name = "Nouveau mot de passe")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Compare("NouveauMotDePasse", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmerMotDePasse { get; set; } = string.Empty;
    }
}
