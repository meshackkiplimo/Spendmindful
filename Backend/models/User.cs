namespace Backend.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DailyEntry> DailyEntries { get; set; } = [];
    public List<Transaction> Transactions { get; set; } = [];
}