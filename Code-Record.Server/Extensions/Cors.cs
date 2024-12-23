namespace Code_Record.Server.Extensions;
public static class Cors
{
	public static IServiceCollection AllowedApps(this IServiceCollection services)
	{
		services.AddCors(options =>
		{
			options.AddPolicy("AllowedApps", builder =>
			{
				builder
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod();
			});
		});
		return services;
	}
}
