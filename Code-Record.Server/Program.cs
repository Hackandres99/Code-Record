using Code_Record.Server.Extensions;
using Code_Record.Server.Extensions.Contexts.Database;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddConfigControllers();

			builder.Services.AddAuthToken(builder.Configuration);

			builder.Services.AddEndpointsApiExplorer();

			builder.DatabaseType();

			builder.Services.AllowedApps();

			var app = builder.Build();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.UseCors("AllowedApps");

			app.MapControllers();

			app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}
