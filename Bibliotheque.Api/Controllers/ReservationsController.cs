using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReservationService _reservationService;

        public ReservationsController(IUnitOfWork unitOfWork, IReservationService reservationService)
        {
            _unitOfWork = unitOfWork;
            _reservationService = reservationService;
        }

        /// <summary>
        /// Obtenir les réservations d'un utilisateur
        /// </summary>
        [HttpGet("utilisateur/{idUtilisateur}")]
        public async Task<ActionResult<IEnumerable<ReservationDTO>>> GetByUtilisateur(
            int idUtilisateur,
            [FromQuery] bool activeUniquement = true)
        {
            var reservations = await _unitOfWork.Reservations.GetByUtilisateurAsync(idUtilisateur, activeUniquement);

            var dtos = reservations.Select(r => new ReservationDTO
            {
                IdReservation = r.IdReservation,
                IdLivre = r.IdLivre,
                LivreTitre = r.Livre?.Titre ?? "",
                LivreImage = r.Livre?.ImageCouverture,
                IdUtilisateur = r.IdUtilisateur,
                UtilisateurNom = r.Utilisateur?.NomComplet ?? "",
                DateReservation = r.DateReservation,
                DateExpiration = r.DateExpiration,
                PositionFile = r.PositionFile,
                Statut = r.Statut
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Créer une réservation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Reserver([FromBody] ReservationCreateDTO dto)
        {
            var (succes, message, idReservation, position) = await _reservationService.ReserverAsync(
                dto.IdLivre, dto.IdUtilisateur);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, idReservation, position });
        }

        /// <summary>
        /// Annuler une réservation
        /// </summary>
        [HttpDelete("{idReservation}")]
        public async Task<ActionResult> Annuler(int idReservation)
        {
            var (succes, message) = await _reservationService.AnnulerReservationAsync(idReservation);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Convertir une réservation en emprunt
        /// </summary>
        [HttpPost("{idReservation}/convertir")]
        public async Task<ActionResult> Convertir(int idReservation)
        {
            var (succes, message, idEmprunt) = await _reservationService.ConvertirEnEmpruntAsync(idReservation);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, idEmprunt });
        }

        /// <summary>
        /// Obtenir la position dans la file d'attente
        /// </summary>
        [HttpGet("position")]
        public async Task<ActionResult> GetPosition([FromQuery] int idLivre, [FromQuery] int idUtilisateur)
        {
            var position = await _unitOfWork.Reservations.GetPositionFileAsync(idLivre, idUtilisateur);
            return Ok(new { position });
        }
    }
}
