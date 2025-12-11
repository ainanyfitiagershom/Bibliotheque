using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtenir les notifications d'un utilisateur
        /// </summary>
        [HttpGet("utilisateur/{idUtilisateur}")]
        public async Task<ActionResult> GetByUtilisateur(int idUtilisateur, [FromQuery] bool nonLuesUniquement = false)
        {
            var notifications = await _unitOfWork.Notifications.GetByUtilisateurAsync(idUtilisateur, nonLuesUniquement);

            var dtos = notifications.Select(n => new
            {
                n.IdNotification,
                n.Type,
                n.Titre,
                n.Message,
                n.DateCreation,
                n.EstLue,
                n.Lien,
                TypeIcone = n.TypeIcone,
                TypeCouleur = n.TypeCouleur,
                TempsEcoule = n.TempsEcoule
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Compter les notifications non lues
        /// </summary>
        [HttpGet("utilisateur/{idUtilisateur}/count")]
        public async Task<ActionResult> CountNonLues(int idUtilisateur)
        {
            var count = await _unitOfWork.Notifications.CompterNonLuesAsync(idUtilisateur);
            return Ok(new { count });
        }

        /// <summary>
        /// Marquer une notification comme lue
        /// </summary>
        [HttpPost("{idNotification}/lue")]
        public async Task<ActionResult> MarquerCommeLue(int idNotification)
        {
            await _unitOfWork.Notifications.MarquerCommeLueAsync(idNotification);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Notification marquée comme lue" });
        }

        /// <summary>
        /// Marquer toutes les notifications comme lues
        /// </summary>
        [HttpPost("utilisateur/{idUtilisateur}/lues")]
        public async Task<ActionResult> MarquerToutesCommeLues(int idUtilisateur)
        {
            await _unitOfWork.Notifications.MarquerToutesCommeLuesAsync(idUtilisateur);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Toutes les notifications ont été marquées comme lues" });
        }

        /// <summary>
        /// Supprimer une notification
        /// </summary>
        [HttpDelete("{idNotification}")]
        public async Task<ActionResult> Supprimer(int idNotification)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(idNotification);
            if (notification == null)
            {
                return NotFound(new { message = "Notification non trouvée" });
            }

            await _unitOfWork.Notifications.DeleteAsync(notification);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Notification supprimée" });
        }
    }
}
