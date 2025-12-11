using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Utilisateurs")]
    public class Utilisateur
    {
        [Key]
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
        public DateTime? DateNaissance { get; set; }

        [Required]
        [StringLength(255)]
        public string MotDePasseHash { get; set; } = string.Empty;

        [Display(Name = "Date d'inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        [Display(Name = "Dernière connexion")]
        public DateTime? DerniereConnexion { get; set; }

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

        // Navigation
        public virtual ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Avis> Avis { get; set; } = new List<Avis>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Propriétés alias pour compatibilité avec les vues
        [NotMapped]
        public string NumeroAbonne => $"AB{IdUtilisateur:D6}";

        [NotMapped]
        public string Statut => !Actif ? "Inactif" : EstBloque ? "Bloqué" : "Actif";

        // Propriétés calculées
        [NotMapped]
        public string NomComplet => $"{Prenom} {Nom}";

        [NotMapped]
        public int NombreEmpruntsEnCours => Emprunts?.Count(e => e.Statut == "EnCours" || e.Statut == "EnRetard") ?? 0;

        [NotMapped]
        public bool PeutEmprunter => Actif && !EstBloque && NombreEmpruntsEnCours < NombreEmpruntsMax;
    }
}
