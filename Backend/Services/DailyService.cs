using Backend.DTOs;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DailyService
{
    private readonly AppDbContext _db;

    public DailyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DailyEntry> GetOrCreateTodayAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var entry = await _db.DailyEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

        if (entry == null)
        {
            entry = new DailyEntry { UserId = userId, Date = today, PlannedAmount = 0 };
            _db.DailyEntries.Add(entry);
            await _db.SaveChangesAsync();
        }

        return entry;
    }

    public async Task<DailyEntry> SetPlannedAmountAsync(int userId, PlannedDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var entry = await _db.DailyEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

        if (entry == null)
        {
            entry = new DailyEntry { UserId = userId, Date = today };
            _db.DailyEntries.Add(entry);
        }

        entry.PlannedAmount = dto.PlannedAmount;
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<DailyEntry?> UpdateEveningAsync(int userId, EveningDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var entry = await _db.DailyEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

        if (entry == null) return null;

        entry.ActualAmount = dto.ActualAmount;
        entry.StuckToPlan = dto.StuckToPlan;
        entry.Reflection = dto.Reflection;
        entry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return entry;
    }
}
