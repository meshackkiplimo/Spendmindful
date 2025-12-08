using Backend.Controllers;
using Backend.DTOs;

namespace Backend.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async (RegisterDto dto, AuthController controller) =>
            await controller.Register(dto));

        app.MapPost("/api/auth/login", async (LoginDto dto, AuthController controller) =>
            await controller.Login(dto));
    }
}
