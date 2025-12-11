using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;

namespace Bibliotheque.Core.Interfaces
{
    /// <summary>
    /// Service de gestion des emprunts
    /// </summary>
    public interface IEmpruntService
    {
        Task<(bool Succes, string Message, int? IdEmprunt)> EffectuerEmpruntAsync(int idLivre, int idUtilisateur, int dureeJours = 14);
        Task<(bool Succes, string Message, decimal Penalite)> EffectuerRetourAsync(int idEmprunt);
        Task<(bool Succes, string Message)> ProlongerEmpruntAsync(int idEmprunt, int nombreJours = 7);
        Task DetecterRetardsAsync();
    }

    /// <summary>
    /// Service de gestion des réservations
    /// </summary>
    public interface IReservationService
    {
        Task<(bool Succes, string Message, int? IdReservation, int Position)> ReserverAsync(int idLivre, int idUtilisateur);
        Task<(bool Succes, string Message)> AnnulerReservationAsync(int idReservation);
        Task<(bool Succes, string Message, int? IdEmprunt)> ConvertirEnEmpruntAsync(int idReservation);
        Task ExpirerReservationsAsync();
    }

    /// <summary>
    /// Service d'authentification
    /// </summary>
    public interface IAuthService
    {
        Task<(bool Succes, Admin? Admin, string Message)> AuthentifierAdminAsync(string email, string motDePasse);
        Task<(bool Succes, Utilisateur? Utilisateur, string Message)> AuthentifierUtilisateurAsync(string email, string motDePasse);
        Task<(bool Succes, string Message)> InscrireUtilisateurAsync(InscriptionDTO inscription);
        Task<(bool Succes, string Message)> ChangerMotDePasseAsync(int idUtilisateur, string ancienMdp, string nouveauMdp);
        string HashMotDePasse(string motDePasse);
        bool VerifierMotDePasse(string motDePasse, string hash);
    }

    /// <summary>
    /// Service d'import/export
    /// </summary>
    public interface IImportExportService
    {
        Task<ImportResultDTO> ImporterLivresCsvAsync(Stream fichierCsv);
        Task<ImportResultDTO> ImporterAuteursCsvAsync(Stream fichierCsv);
        Task<ImportResultDTO> ImporterUtilisateursCsvAsync(Stream fichierCsv);
        Task<byte[]> ExporterLivresPdfAsync();
        Task<byte[]> ExporterEmpruntsPdfAsync(EmpruntFiltreDTO? filtre = null);
        Task<byte[]> ExporterUtilisateursPdfAsync();
        Task<byte[]> ExporterRapportActivitePdfAsync(DateTime dateDebut, DateTime dateFin);
    }

    /// <summary>
    /// Service de statistiques
    /// </summary>
    public interface IStatistiquesService
    {
        Task<DashboardStatsDTO> GetDashboardStatsAsync();
        Task<IEnumerable<TopLivreDTO>> GetTopLivresAsync(int nombre = 10);
        Task<IEnumerable<EmpruntParMoisDTO>> GetEmpruntsParMoisAsync(int nombreMois = 12);
        Task<IEnumerable<EmpruntParCategorieDTO>> GetEmpruntsParCategorieAsync();
        Task<RapportActiviteDTO> GetRapportActiviteAsync(DateTime dateDebut, DateTime dateFin);
    }

    /// <summary>
    /// Service de notifications
    /// </summary>
    public interface INotificationService
    {
        Task EnvoyerNotificationAsync(int idUtilisateur, string type, string titre, string message, string? lien = null);
        Task EnvoyerNotificationsRetardAsync();
        Task EnvoyerRappelsEcheanceAsync(int joursAvant = 2);
        Task NettoyerAnciennesNotificationsAsync(int joursAnciennete = 30);
    }

    /// <summary>
    /// Service de recommandations
    /// </summary>
    public interface IRecommandationService
    {
        Task<IEnumerable<LivreDTO>> GetRecommandationsAsync(int idUtilisateur, int nombre = 5);
        Task<IEnumerable<LivreDTO>> GetLivresSimilairesAsync(int idLivre, int nombre = 5);
    }
}
