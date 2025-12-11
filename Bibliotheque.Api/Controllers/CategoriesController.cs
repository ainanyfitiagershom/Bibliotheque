using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtenir toutes les catégories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var categories = await _unitOfWork.Categories.GetAllWithCountAsync();

            var dtos = categories.Select(c => new
            {
                c.IdCategorie,
                c.Nom,
                c.Description,
                c.Couleur,
                c.Icone,
                NombreLivres = c.LivreCategories.Count
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Obtenir une catégorie par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var categorie = await _unitOfWork.Categories.GetByIdWithLivresAsync(id);

            if (categorie == null || !categorie.Actif)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return Ok(new
            {
                categorie.IdCategorie,
                categorie.Nom,
                categorie.Description,
                categorie.Couleur,
                categorie.Icone,
                NombreLivres = categorie.LivreCategories.Count
            });
        }
    }
}
