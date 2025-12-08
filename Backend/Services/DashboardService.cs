using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> GetDashboardAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);

        var balance = await _db.Transactions
            .Where(t => t.UserId == userId)
            .SumAsync(t => t.Amount);

        var monthlySpend = await _db.Transactions
            .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Amount < 0)
            .SumAsync(t => t.Amount);

        var successRate = await _db.DailyEntries
            .Where(d => d.UserId == userId && d.StuckToPlan == true)
            .CountAsync();

        var totalDays = await _db.DailyEntries
            .Where(d => d.UserId == userId && d.StuckToPlan != null)
            .CountAsync();

        return new
        {
            balance,
            monthlySpend = Math.Abs(monthlySpend),
            successRate = totalDays > 0 ? Math.Round((double)successRate / totalDays * 100, 1) : 0
        };
    }
}
