using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoPro24Team06.Models;

//-------------------------
// Author: Michael Adolf
//-------------------------

public static class UserExtensions
{
    /// <summary>
    /// Checks if the user is locked out
    /// </summary>
    public static async Task<bool> IsLockedOutAsync(
        this ClaimsPrincipal user,
        IServiceProvider services
    )
    {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            return true; // User is not authenticated
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var applicationUser = await userManager.GetUserAsync(user);

        if (applicationUser == null)
        {
            return true; // User not found
        }

        return await userManager.IsLockedOutAsync(applicationUser);
    }
}
