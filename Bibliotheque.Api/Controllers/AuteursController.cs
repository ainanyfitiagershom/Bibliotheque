using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuteursController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuteursController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtenir tous les auteurs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var auteurs = await _unitOfWork.Auteurs.FindAsync(a => a.Actif);

            var dtos = auteurs.Select(a => new
            {
                a.IdAuteur,
                a.Nom,
                a.Prenom,
                NomComplet = a.NomComplet,
                a.Nationalite,
                a.PhotoUrl,
                NombreLivres = a.Livres?.Count(l => l.Actif) ?? 0
            }).OrderBy(a => a.Nom);

            return Ok(dtos);
        }

        /// <summary>
        /// Obtenir un auteur par ID avec ses livres
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var auteur = await _unitOfWork.Auteurs.GetByIdWithLivresAsync(id);

            if (auteur == null || !auteur.Actif)
            {
                return NotFound(new { message = "Auteur non trouvé" });
            }

            return Ok(new
            {
                auteur.IdAuteur,
                auteur.Nom,
                auteur.Prenom,
                NomComplet = auteur.NomComplet,
                auteur.Nationalite,
                auteur.DateNaissance,
                auteur.DateDeces,
                auteur.Biographie,
                auteur.PhotoUrl,
                Livres = auteur.Livres.Where(l => l.Actif).Select(l => new
                {
                    l.IdLivre,
                    l.Titre,
                    l.Annee,
                    l.ImageCouverture,
                    l.StockDisponible,
                    l.NoteMoyenne
                })
            });
        }

        /// <summary>
        /// Rechercher des auteurs
        /// </summary>
        [HttpGet("recherche")]
        public async Task<ActionResult> Rechercher([FromQuery] string terme)
        {
            if (string.IsNullOrWhiteSpace(terme))
            {
                return Ok(new List<object>());
            }

            var auteurs = await _unitOfWork.Auteurs.RechercherAsync(terme);

            var dtos = auteurs.Select(a => new
            {
                a.IdAuteur,
                a.Nom,
                a.Prenom,
                NomComplet = a.NomComplet,
                a.PhotoUrl
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Obtenir les auteurs les plus populaires
        /// </summary>
        [HttpGet("populaires")]
        public async Task<ActionResult> GetPopulaires([FromQuery] int nombre = 10)
        {
            var auteurs = await _unitOfWork.Auteurs.GetPopulairesAsync(nombre);

            var dtos = auteurs.Select(a => new
            {
                a.IdAuteur,
                NomComplet = a.NomComplet,
                a.PhotoUrl,
                TotalEmprunts = a.Livres?.Sum(l => l.NombreEmprunts) ?? 0
            });

            return Ok(dtos);
        }
    }
}
