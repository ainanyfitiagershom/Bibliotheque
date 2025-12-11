using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IStatistiquesService _statistiquesService;

        public IndexModel(IStatistiquesService statistiquesService)
        {
            _statistiquesService = statistiquesService;
        }

        public DashboardStatsDTO Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _statistiquesService.GetDashboardStatsAsync();
        }
    }
}
