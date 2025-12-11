using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Connexion utilisateur (frontoffice)
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginUtilisateurDTO dto)
        {
            var (succes, utilisateur, message) = await _authService.AuthentifierUtilisateurAsync(dto.Email, dto.MotDePasse);

            if (!succes)
            {
                return Unauthorized(new { message });
            }

            return Ok(new
            {
                message,
                utilisateur = new
                {
                    utilisateur!.IdUtilisateur,
                    utilisateur.Nom,
                    utilisateur.Prenom,
                    utilisateur.Email,
                    utilisateur.NombreEmpruntsMax
                }
            });
        }

        /// <summary>
        /// Inscription utilisateur
        /// </summary>
        [HttpPost("inscription")]
        public async Task<ActionResult> Inscription([FromBody] InscriptionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (succes, message) = await _authService.InscrireUtilisateurAsync(dto);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Changer le mot de passe
        /// </summary>
        [HttpPost("changer-mot-de-passe")]
        public async Task<ActionResult> ChangerMotDePasse([FromBody] ChangerMotDePasseDTO dto, [FromQuery] int idUtilisateur)
        {
            var (succes, message) = await _authService.ChangerMotDePasseAsync(
                idUtilisateur, dto.AncienMotDePasse, dto.NouveauMotDePasse);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Obtenir le profil utilisateur
        /// </summary>
        [HttpGet("profil/{id}")]
        public async Task<ActionResult> GetProfil(int id)
        {
            var utilisateur = await _unitOfWork.Utilisateurs.GetByIdWithDetailsAsync(id);

            if (utilisateur == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            return Ok(new
            {
                utilisateur.IdUtilisateur,
                utilisateur.Nom,
                utilisateur.Prenom,
                utilisateur.Email,
                utilisateur.Telephone,
                utilisateur.Adresse,
                utilisateur.DateInscription,
                utilisateur.NombreEmpruntsMax,
                EmpruntsEnCours = utilisateur.Emprunts.Count(e => e.Statut == "EnCours" || e.Statut == "EnRetard"),
                ReservationsActives = utilisateur.Reservations.Count,
                NotificationsNonLues = utilisateur.Notifications.Count
            });
        }

        /// <summary>
        /// Mettre à jour le profil utilisateur
        /// </summary>
        [HttpPut("profil/{id}")]
        public async Task<ActionResult> UpdateProfil(int id, [FromBody] dynamic dto)
        {
            var utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            // Mettre à jour les champs modifiables
            if (dto.telephone != null) utilisateur.Telephone = dto.telephone;
            if (dto.adresse != null) utilisateur.Adresse = dto.adresse;

            await _unitOfWork.Utilisateurs.UpdateAsync(utilisateur);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Profil mis à jour avec succès" });
        }
    }
}
