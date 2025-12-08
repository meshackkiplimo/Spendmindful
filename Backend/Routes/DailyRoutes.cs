using Backend.Controllers;
using Backend.DTOs;

namespace Backend.Routes;

public static class DailyRoutes
{
    public static void MapDailyRoutes(this WebApplication app)
    {
        app.MapGet("/api/daily/today", async (HttpContext ctx, DailyController controller) =>
            await controller.GetToday(ctx)).RequireAuthorization();

        app.MapPost("/api/daily/planned", async (PlannedDto dto, HttpContext ctx, DailyController controller) =>
            await controller.SetPlanned(dto, ctx)).RequireAuthorization();

        app.MapPut("/api/daily/evening", async (EveningDto dto, HttpContext ctx, DailyController controller) =>
            await controller.UpdateEvening(dto, ctx)).RequireAuthorization();
    }
}
