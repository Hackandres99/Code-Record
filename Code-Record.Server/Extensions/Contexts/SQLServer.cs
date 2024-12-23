using Code_Record.Server.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Code_Record.Server.Extensions.Contexts;

public static class SQLServer
{
	public static IServiceCollection AddSQLServerContext(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("SQLServer");
		services.AddDbContext <SQLServerContext>(options => options.UseSqlServer(connectionString));
		return services;
	}
}
