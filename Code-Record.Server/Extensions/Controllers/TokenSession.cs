using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Code_Record.Server.Extensions.Controllers
{
    public static class TokenSession
    {
        public static IServiceCollection AddAuthToken(this IServiceCollection service, IConfiguration configuration)
        {
            var secretKey = Encoding.ASCII.GetBytes(configuration["Auth:Key"]);
            service.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = e =>
                    {
                        Console.WriteLine($"Token error validation: {e.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = t =>
                    {
                        Console.WriteLine($"Token: {t.SecurityToken}");
                        return Task.CompletedTask;
                    }
                };
            });
            return service;
        }
    }
}
