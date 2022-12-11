using Microsoft.Extensions.Primitives;
using System.Drawing.Text;

namespace Varausharjoitus.Middleware
{
    public class ApiKeyMiddleware
    {

        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "ApiKey";

        //konstruktori
        public ApiKeyMiddleware(RequestDelegate next)
        {
         _next = next;
        }

        //käsittely
        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api key missing");
                return;
            }
            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<String>(APIKEYNAME);
            if(!apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unautorized client");
                return;
            }
            await _next(context);
        }
    }
}
