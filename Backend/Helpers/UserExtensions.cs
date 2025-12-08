using System.Security.Claims;

namespace Backend.Helpers;

public static class UserExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirst("userId")!.Value);
    }
}
