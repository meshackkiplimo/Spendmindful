using Backend.DTOs;
using Backend.Helpers;
using Backend.Services;

namespace Backend.Controllers;

public class DailyController
{
    private readonly DailyService _dailyService;

    public DailyController(DailyService dailyService)
    {
        _dailyService = dailyService;
    }

    public async Task<IResult> GetToday(HttpContext ctx)
    {
        var userId = ctx.User.GetUserId();
        var entry = await _dailyService.GetOrCreateTodayAsync(userId);
        return Results.Ok(entry);
    }

    public async Task<IResult> SetPlanned(PlannedDto dto, HttpContext ctx)
    {
        var userId = ctx.User.GetUserId();
        var entry = await _dailyService.SetPlannedAmountAsync(userId, dto);
        return Results.Ok(entry);
    }

    public async Task<IResult> UpdateEvening(EveningDto dto, HttpContext ctx)
    {
        var userId = ctx.User.GetUserId();
        var entry = await _dailyService.UpdateEveningAsync(userId, dto);
        return entry != null ? Results.Ok(entry) : Results.NotFound();
    }
}
