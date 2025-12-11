using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LivresController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public LivresController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Rechercher des livres avec filtres et pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResultDTO<LivreDTO>>> GetLivres([FromQuery] LivreRechercheDTO recherche)
        {
            var result = await _unitOfWork.Livres.RechercherAsync(recherche);
            return Ok(result);
        }

        /// <summary>
        /// Obtenir un livre par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LivreDTO>> GetLivre(int id)
        {
            var livre = await _unitOfWork.Livres.GetByIdWithDetailsAsync(id);

            if (livre == null || !livre.Actif)
            {
                return NotFound(new { message = "Livre non trouvé" });
            }

            var dto = new LivreDTO
            {
                IdLivre = livre.IdLivre,
                ISBN = livre.ISBN,
                Titre = livre.Titre,
                IdAuteur = livre.IdAuteur,
                AuteurNom = livre.Auteur?.NomComplet ?? "",
                Annee = livre.Annee,
                Editeur = livre.Editeur,
                NombrePages = livre.NombrePages,
                Langue = livre.Langue,
                Description = livre.Description,
                ImageCouverture = livre.ImageCouverture,
                Stock = livre.Stock,
                StockDisponible = livre.StockDisponible,
                Emplacement = livre.Emplacement,
                NombreEmprunts = livre.NombreEmprunts,
                NoteMoyenne = livre.NoteMoyenne,
                Categories = livre.LivreCategories.Select(lc => new CategorieSimpleDTO
                {
                    IdCategorie = lc.Categorie?.IdCategorie ?? 0,
                    Nom = lc.Categorie?.Nom ?? "",
                    Couleur = lc.Categorie?.Couleur
                }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Recherche rapide pour auto-complétion
        /// </summary>
        [HttpGet("recherche-rapide")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> RechercheRapide([FromQuery] string terme)
        {
            if (string.IsNullOrWhiteSpace(terme) || terme.Length < 2)
            {
                return Ok(new List<LivreDTO>());
            }

            var livres = await _unitOfWork.Livres.RechercheRapideAsync(terme);
            return Ok(livres);
        }

        /// <summary>
        /// Obtenir les nouveautés
        /// </summary>
        [HttpGet("nouveautes")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> GetNouveautes([FromQuery] int nombre = 10)
        {
            var livres = await _unitOfWork.Livres.GetNouveautesAsync(nombre);
            return Ok(livres.Select(l => new LivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                AuteurNom = l.Auteur?.NomComplet ?? "",
                ImageCouverture = l.ImageCouverture,
                StockDisponible = l.StockDisponible,
                NoteMoyenne = l.NoteMoyenne
            }));
        }

        /// <summary>
        /// Obtenir les livres populaires
        /// </summary>
        [HttpGet("populaires")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> GetPopulaires([FromQuery] int nombre = 10)
        {
            var livres = await _unitOfWork.Livres.GetPopulairesAsync(nombre);
            return Ok(livres.Select(l => new LivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                AuteurNom = l.Auteur?.NomComplet ?? "",
                ImageCouverture = l.ImageCouverture,
                StockDisponible = l.StockDisponible,
                NombreEmprunts = l.NombreEmprunts,
                NoteMoyenne = l.NoteMoyenne
            }));
        }

        /// <summary>
        /// Obtenir les livres les mieux notés
        /// </summary>
        [HttpGet("mieux-notes")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> GetMieuxNotes([FromQuery] int nombre = 10)
        {
            var livres = await _unitOfWork.Livres.GetMieuxNotesAsync(nombre);
            return Ok(livres.Select(l => new LivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                AuteurNom = l.Auteur?.NomComplet ?? "",
                ImageCouverture = l.ImageCouverture,
                StockDisponible = l.StockDisponible,
                NoteMoyenne = l.NoteMoyenne
            }));
        }

        /// <summary>
        /// Obtenir les livres par catégorie
        /// </summary>
        [HttpGet("categorie/{idCategorie}")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> GetByCategorie(int idCategorie)
        {
            var livres = await _unitOfWork.Livres.GetByCategorieAsync(idCategorie);
            return Ok(livres.Select(l => new LivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                AuteurNom = l.Auteur?.NomComplet ?? "",
                Annee = l.Annee,
                ImageCouverture = l.ImageCouverture,
                StockDisponible = l.StockDisponible,
                NoteMoyenne = l.NoteMoyenne
            }));
        }

        /// <summary>
        /// Obtenir les livres par auteur
        /// </summary>
        [HttpGet("auteur/{idAuteur}")]
        public async Task<ActionResult<IEnumerable<LivreDTO>>> GetByAuteur(int idAuteur)
        {
            var livres = await _unitOfWork.Livres.GetByAuteurAsync(idAuteur);
            return Ok(livres.Select(l => new LivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                AuteurNom = l.Auteur?.NomComplet ?? "",
                Annee = l.Annee,
                ImageCouverture = l.ImageCouverture,
                StockDisponible = l.StockDisponible,
                NoteMoyenne = l.NoteMoyenne
            }));
        }
    }
}
