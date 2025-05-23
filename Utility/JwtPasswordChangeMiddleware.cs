using RideWild.Models.DataModels;
using System.Security.Claims;

namespace RideWild.Utility
{
    public class JwtPasswordChangeMiddleware
    {
            private readonly RequestDelegate _next;

            public JwtPasswordChangeMiddleware(RequestDelegate next)
            {
                _next = next;
            }

        public async Task InvokeAsync(HttpContext context, AdventureWorksDataContext dbContext)
        {
            Console.WriteLine("🔐 MIDDLEWARE CHIAMATO");

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenPwdChangeStr = context.User.FindFirst("lastPasswordChange")?.Value;

                Console.WriteLine($"➡ User ID dal token: {userIdStr}");
                Console.WriteLine($"➡ PasswordChange dal token (string): {tokenPwdChangeStr}");

                if (long.TryParse(userIdStr, out long userId) &&
                    DateTime.TryParse(tokenPwdChangeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var tokenPwdChangeUtc))
                {
                    var customer = await dbContext.CustomerData.FindAsync(userId);

                    if (customer != null && customer.LastPasswordChange.HasValue)
                    {
                        var dbPwdChangeUtc = customer.LastPasswordChange.Value;

                        // Forza entrambi in UTC per confronto affidabile
                        var dbTime = DateTime.SpecifyKind(dbPwdChangeUtc, DateTimeKind.Utc);
                        var tokenTime = DateTime.SpecifyKind(tokenPwdChangeUtc, DateTimeKind.Utc);

                        Console.WriteLine($"🕒 TokenTime: {tokenTime:o} (Kind: {tokenTime.Kind})");
                        Console.WriteLine($"🕒 DBTime: {dbTime:o} (Kind: {dbTime.Kind})");

                        if (tokenTime < dbTime)
                        {
                            Console.WriteLine("⛔ Token NON valido: password è stata cambiata DOPO la generazione del token.");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Token non più valido: la password è stata modificata.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("✅ Token valido: password non è stata modificata dopo il token.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Utente non trovato o LastPasswordChange nullo");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Errore parsing UserId o tokenPwdChangeUtc");
                }
            }

            await _next(context);
        }

    }

}
