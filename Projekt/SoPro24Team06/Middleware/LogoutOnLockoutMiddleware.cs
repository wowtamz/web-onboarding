using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using SoPro24Team06.Models;

//-------------------------
// Author: Michael Adolf
//-------------------------

public class LogoutOnLockoutMiddleware
{
    private readonly RequestDelegate _next;

    public LogoutOnLockoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user != null && await userManager.IsLockedOutAsync(user))
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Identity/Account/Lockout");
                return;
            }
        }

        await _next(context);
    }
}
