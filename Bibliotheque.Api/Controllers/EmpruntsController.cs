using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpruntsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmpruntService _empruntService;

        public EmpruntsController(IUnitOfWork unitOfWork, IEmpruntService empruntService)
        {
            _unitOfWork = unitOfWork;
            _empruntService = empruntService;
        }

        /// <summary>
        /// Obtenir les emprunts d'un utilisateur
        /// </summary>
        [HttpGet("utilisateur/{idUtilisateur}")]
        public async Task<ActionResult<IEnumerable<EmpruntDTO>>> GetByUtilisateur(
            int idUtilisateur,
            [FromQuery] bool enCoursUniquement = false)
        {
            var emprunts = await _unitOfWork.Emprunts.GetByUtilisateurAsync(idUtilisateur, enCoursUniquement);

            var dtos = emprunts.Select(e => new EmpruntDTO
            {
                IdEmprunt = e.IdEmprunt,
                IdLivre = e.IdLivre,
                LivreTitre = e.Livre?.Titre ?? "",
                LivreImage = e.Livre?.ImageCouverture,
                IdUtilisateur = e.IdUtilisateur,
                UtilisateurNom = e.Utilisateur?.NomComplet ?? "",
                DateEmprunt = e.DateEmprunt,
                DateRetourPrevue = e.DateRetourPrevue,
                DateRetourEffective = e.DateRetourEffective,
                Statut = e.Statut,
                NombreProlongations = e.NombreProlongations,
                MaxProlongations = e.MaxProlongations,
                Penalite = e.Penalite
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Effectuer un emprunt
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Emprunter([FromBody] EmpruntCreateDTO dto)
        {
            var (succes, message, idEmprunt) = await _empruntService.EffectuerEmpruntAsync(
                dto.IdLivre, dto.IdUtilisateur, dto.DureeJours);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, idEmprunt });
        }

        /// <summary>
        /// Effectuer un retour
        /// </summary>
        [HttpPost("{idEmprunt}/retour")]
        public async Task<ActionResult> Retourner(int idEmprunt)
        {
            var (succes, message, penalite) = await _empruntService.EffectuerRetourAsync(idEmprunt);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, penalite });
        }

        /// <summary>
        /// Prolonger un emprunt
        /// </summary>
        [HttpPost("{idEmprunt}/prolonger")]
        public async Task<ActionResult> Prolonger(int idEmprunt, [FromQuery] int jours = 7)
        {
            var (succes, message) = await _empruntService.ProlongerEmpruntAsync(idEmprunt, jours);

            if (!succes)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Obtenir les détails d'un emprunt
        /// </summary>
        [HttpGet("{idEmprunt}")]
        public async Task<ActionResult<EmpruntDTO>> GetEmprunt(int idEmprunt)
        {
            var emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(idEmprunt);

            if (emprunt == null)
            {
                return NotFound(new { message = "Emprunt non trouvé" });
            }

            var dto = new EmpruntDTO
            {
                IdEmprunt = emprunt.IdEmprunt,
                IdLivre = emprunt.IdLivre,
                LivreTitre = emprunt.Livre?.Titre ?? "",
                LivreImage = emprunt.Livre?.ImageCouverture,
                IdUtilisateur = emprunt.IdUtilisateur,
                UtilisateurNom = emprunt.Utilisateur?.NomComplet ?? "",
                UtilisateurEmail = emprunt.Utilisateur?.Email ?? "",
                DateEmprunt = emprunt.DateEmprunt,
                DateRetourPrevue = emprunt.DateRetourPrevue,
                DateRetourEffective = emprunt.DateRetourEffective,
                Statut = emprunt.Statut,
                NombreProlongations = emprunt.NombreProlongations,
                MaxProlongations = emprunt.MaxProlongations,
                Penalite = emprunt.Penalite,
                Notes = emprunt.Notes
            };

            return Ok(dto);
        }
    }
}
