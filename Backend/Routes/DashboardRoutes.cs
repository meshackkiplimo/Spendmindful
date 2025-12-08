using Backend.Controllers;

namespace Backend.Routes;

public static class DashboardRoutes
{
    public static void MapDashboardRoutes(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (HttpContext ctx, DashboardController controller) =>
            await controller.GetDashboard(ctx)).RequireAuthorization();
    }
}
