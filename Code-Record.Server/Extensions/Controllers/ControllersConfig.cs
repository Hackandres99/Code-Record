using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Code_Record.Server.Extensions.Controllers
{
    public static class ControllersConfig
    {
        public static IServiceCollection AddConfigControllers(this IServiceCollection services)
        {
            services.AddControllers(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));

				config.Filters.Add(new AdminAuthorize(
		            nameof(Models.Base.UserStructure.RolOptions.Admin),
		            "/api/sql/admin",
		            "/api/mongo/admin"
	            ));

			}).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new ObjectIdConverter());
            });

            return services;
        }
    }
}
