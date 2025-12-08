using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyEntry> DailyEntries => Set<DailyEntry>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyEntry>()
            .HasIndex(d => new { d.UserId, d.Date })
            .IsUnique();
    }
}