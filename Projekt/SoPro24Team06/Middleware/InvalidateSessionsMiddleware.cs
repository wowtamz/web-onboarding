public class InvalidateSessionsMiddleware
{
    private readonly RequestDelegate _next;

    public InvalidateSessionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Überprüfen, ob der Benutzer authentifiziert ist
        if (context.User.Identity.IsAuthenticated)
        {
            // Benutzer abmelden
            await context.SignOutAsync();
        }

        // Weiter zur nächsten Middleware
        await _next(context);
    }
}
