using Frontoffice.MVC.Models;
using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontoffice.MVC.Controllers
{
    public class CompteController : Controller
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly IEmpruntService _empruntService;
        private readonly IReservationService _reservationService;
        private readonly INotificationService _notificationService;

        public CompteController(
            IUtilisateurService utilisateurService,
            IEmpruntService empruntService,
            IReservationService reservationService,
            INotificationService notificationService)
        {
            _utilisateurService = utilisateurService;
            _empruntService = empruntService;
            _reservationService = reservationService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _utilisateurService.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.IdUtilisateur.ToString()),
                new(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
                new(ClaimTypes.Email, user.Email),
                new("NumeroAbonne", user.NumeroAbonne ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _utilisateurService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            var success = await _utilisateurService.RegisterAsync(
                model.Nom, model.Prenom, model.Email, model.Password, model.Telephone);

            if (!success)
            {
                ModelState.AddModelError("", "Erreur lors de l'inscription.");
                return View(model);
            }

            TempData["Success"] = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var user = await _utilisateurService.GetByIdAsync(userId);

            if (user == null) return NotFound();

            var viewModel = new CompteIndexViewModel
            {
                Utilisateur = user,
                EmpruntsEnCours = await _empruntService.GetEmpruntsUtilisateurAsync(userId),
                Reservations = await _reservationService.GetReservationsUtilisateurAsync(userId),
                NombreNotificationsNonLues = await _notificationService.GetNombreNonLuesAsync(userId)
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var userId = GetUserId();
            var user = await _utilisateurService.GetByIdAsync(userId);

            if (user == null) return NotFound();

            var model = new ProfilViewModel
            {
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Adresse = user.Adresse,
                NumeroAbonne = user.NumeroAbonne,
                DateInscription = user.DateInscription
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profil(ProfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetUserId();
            var success = await _utilisateurService.UpdateProfileAsync(userId, model.Nom, model.Prenom, model.Telephone, model.Adresse);

            if (success)
                TempData["Success"] = "Profil mis à jour avec succès.";
            else
                TempData["Error"] = "Erreur lors de la mise à jour.";

            return RedirectToAction(nameof(Profil));
        }

        [Authorize]
        public IActionResult ChangerMotDePasse()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangerMotDePasse(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetUserId();
            var success = await _utilisateurService.ChangePasswordAsync(userId, model.AncienMotDePasse, model.NouveauMotDePasse);

            if (success)
            {
                TempData["Success"] = "Mot de passe modifié avec succès.";
                return RedirectToAction(nameof(Profil));
            }

            ModelState.AddModelError("AncienMotDePasse", "Mot de passe actuel incorrect.");
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Historique()
        {
            var userId = GetUserId();
            var historique = await _empruntService.GetHistoriqueUtilisateurAsync(userId);
            return View(historique);
        }

        [Authorize]
        public async Task<IActionResult> Notifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetNotificationsUtilisateurAsync(userId);

            // Marquer toutes comme lues
            await _notificationService.MarquerToutesCommeLuesAsync(userId);

            return View(notifications);
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}
