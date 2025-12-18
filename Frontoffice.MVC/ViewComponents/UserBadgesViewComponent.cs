using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontoffice.MVC.ViewComponents
{
    public class UserBadgesViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IReservationService _reservationService;

        public UserBadgesViewComponent(INotificationService notificationService, IReservationService reservationService)
        {
            _notificationService = notificationService;
            _reservationService = reservationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new UserBadgesViewModel();

            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId) && userId > 0)
                {
                    model.NotificationsCount = await _notificationService.GetNombreNonLuesAsync(userId);
                    var reservations = await _reservationService.GetReservationsUtilisateurAsync(userId);
                    model.ReservationsCount = reservations.Count;
                }
            }

            return View(model);
        }
    }

    public class UserBadgesViewModel
    {
        public int NotificationsCount { get; set; }
        public int ReservationsCount { get; set; }
    }
}
