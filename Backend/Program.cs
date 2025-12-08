using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ======================= API ENDPOINTS =======================

// Register
app.MapPost("/api/auth/register", async (RegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        return Results.BadRequest("Email already exists");

    var user = new User
    {
        Email = dto.Email,
        Name = dto.Name,
        PasswordHash = BCrypt.HashPassword(dto.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Registered successfully" });
});

// Login
app.MapPost("/api/auth/login", async (LoginDto dto, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user == null || !BCrypt.Verify(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = JwtHelper.GenerateToken(user, builder.Configuration);
    return Results.Ok(new { token, user = new { user.Id, user.Email, user.Name } });
});

// Get today's entry (or create if none)
app.MapGet("/api/daily/today", async (HttpContext ctx, AppDbContext db) =>
{
    var userId = ctx.User.GetUserId();
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var entry = await db.DailyEntries
        .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

    if (entry == null)
    {
        entry = new DailyEntry { UserId = userId, Date = today, PlannedAmount = 0 };
        db.DailyEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    return Results.Ok(entry);
});

// Set planned amount (morning)
app.MapPost("/api/daily/planned", async (PlannedDto dto, HttpContext ctx, AppDbContext db) =>
{
    var userId = ctx.User.GetUserId();
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var entry = await db.DailyEntries
        .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

    if (entry == null)
    {
        entry = new DailyEntry { UserId = userId, Date = today };
        db.DailyEntries.Add(entry);
    }

    entry.PlannedAmount = dto.PlannedAmount;
    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

// Evening check-in
app.MapPut("/api/daily/evening", async (EveningDto dto, HttpContext ctx, AppDbContext db) =>
{
    var userId = ctx.User.GetUserId();
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var entry = await db.DailyEntries
        .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == today);

    if (entry == null) return Results.NotFound();

    entry.ActualAmount = dto.ActualAmount;
    entry.StuckToPlan = dto.StuckToPlan;
    entry.Reflection = dto.Reflection;
    entry.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

// Dashboard summary
app.MapGet("/api/dashboard", async (HttpContext ctx, AppDbContext db) =>
{
    var userId = ctx.User.GetUserId();
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var startOfMonth = new DateOnly(today.Year, today.Month, 1);

    var balance = await db.Transactions
        .Where(t => t.UserId == userId)
        .SumAsync(t => t.Amount);

    var monthlySpend = await db.Transactions
        .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Amount < 0)
        .SumAsync(t => t.Amount);

    var successRate = await db.DailyEntries
        .Where(d => d.UserId == userId && d.StuckToPlan == true)
        .CountAsync();

    var totalDays = await db.DailyEntries
        .Where(d => d.UserId == userId && d.StuckToPlan != null)
        .CountAsync();

    return Results.Ok(new
    {
        balance,
        monthlySpend = Math.Abs(monthlySpend),
        successRate = totalDays > 0 ? Math.Round((double)successRate / totalDays * 100, 1) : 0
    });
});

app.Run();

// ================ Helper Classes & Extensions ================

public record RegisterDto(string Email, string Name, string Password);
public record LoginDto(string Email, string Password);
public record PlannedDto(decimal PlannedAmount);
public record EveningDto(decimal ActualAmount, bool StuckToPlan, string? Reflection = null);

public static class JwtHelper
{
    public static string GenerateToken(User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim("userId", user.Id.ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class UserExtensions
{
    public static int GetUserId(this HttpContext ctx)
    {
        return int.Parse(ctx.User.FindFirst("userId")!.Value);
    }
}