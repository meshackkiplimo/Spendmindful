using Backend.DTOs;
using Backend.Helpers;
using Backend.Services;

namespace Backend.Controllers;

public class AuthController
{
    private readonly AuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(AuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    public async Task<IResult> Register(RegisterDto dto)
    {
        var (success, error) = await _authService.RegisterAsync(dto);
        return success ? Results.Ok(new { message = "Registered successfully" }) : Results.BadRequest(error);
    }

    public async Task<IResult> Login(LoginDto dto)
    {
        var user = await _authService.LoginAsync(dto);
        if (user == null) return Results.Unauthorized();

        var token = JwtHelper.GenerateToken(user, _config);
        return Results.Ok(new { token, user = new { user.Id, user.Email, user.Name } });
    }
}
