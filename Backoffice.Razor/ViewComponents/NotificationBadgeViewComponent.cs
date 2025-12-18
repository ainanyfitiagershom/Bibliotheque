using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Razor.ViewComponents
{
    public class NotificationBadgeViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationBadgeViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync();
            var count = notifications.Count(n => !n.EstLue);
            return View(count);
        }
    }
}
