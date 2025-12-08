using Backend.Helpers;
using Backend.Services;

namespace Backend.Controllers;

public class DashboardController
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IResult> GetDashboard(HttpContext ctx)
    {
        var userId = ctx.User.GetUserId();
        var data = await _dashboardService.GetDashboardAsync(userId);
        return Results.Ok(data);
    }
}
