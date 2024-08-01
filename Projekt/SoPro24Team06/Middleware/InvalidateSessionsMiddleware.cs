using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

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
            await context.SignOutAsync();
        }
        await _next(context);
    }
}
