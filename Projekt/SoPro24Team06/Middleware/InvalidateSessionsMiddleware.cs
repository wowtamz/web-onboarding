using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

//-------------------------
// Author: Michael Adolf
//-------------------------

public class InvalidateSessionsMiddleware
{
    private readonly RequestDelegate _next;

    public InvalidateSessionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        }

        await _next(context);
    }
}

