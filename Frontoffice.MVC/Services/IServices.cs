using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;

namespace Frontoffice.MVC.Services
{
    public interface ILivreService
    {
        Task<PagedResultDTO<LivreDTO>> RechercherAsync(string? search, int? categorieId, int page, int pageSize, string tri = "titre");
        Task<LivreDTO?> GetByIdAsync(int id);
        Task<List<Categorie>> GetCategoriesAsync();
        Task<List<LivreDTO>> GetRecommandationsAsync(int userId);
        Task<List<LivreDTO>> GetNouveautesAsync(int count = 10);
        Task<List<LivreDTO>> GetPopulairesAsync(int count = 10);
    }

    public interface IEmpruntService
    {
        Task<List<EmpruntDTO>> GetEmpruntsUtilisateurAsync(int userId);
        Task<List<EmpruntDTO>> GetHistoriqueUtilisateurAsync(int userId);
        Task<(bool Success, string? ErrorMessage)> ProlongerEmpruntAsync(int empruntId, int userId);
        Task<int> GetNombreEmpruntsEnCoursAsync(int userId);
    }

    public interface IUtilisateurService
    {
        Task<Utilisateur?> AuthenticateAsync(string email, string password);
        Task<Utilisateur?> GetByIdAsync(int id);
        Task<bool> RegisterAsync(string nom, string prenom, string email, string password, string? telephone);
        Task<bool> UpdateProfileAsync(int userId, string nom, string prenom, string? telephone, string? adresse);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> EmailExistsAsync(string email);
    }

    public interface IReservationService
    {
        Task<List<ReservationDTO>> GetReservationsUtilisateurAsync(int userId);
        Task<bool> ReserverAsync(int livreId, int userId);
        Task<bool> AnnulerReservationAsync(int reservationId, int userId);
        Task<int> GetPositionFileAttenteAsync(int livreId);
        Task<bool> ADejaReserveAsync(int livreId, int userId);
    }

    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsUtilisateurAsync(int userId);
        Task<int> GetNombreNonLuesAsync(int userId);
        Task MarquerCommeLueAsync(int notificationId, int userId);
        Task MarquerToutesCommeLuesAsync(int userId);
    }
}
