using Code_Record.Server.Extensions.Controllers;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;
using MongoDB.Driver;

namespace Code_Record.Server.Extensions.Contexts;

public static class MongoAtlas
{
	public static IServiceCollection AddMongoAtlasContext(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<IMongoDatabase>(provider =>
		{
			var connectionString = configuration.GetConnectionString("MongoAtlas");
			var client = new MongoClient(connectionString);
			return client.GetDatabase("code_record");
		});

		services.AddScoped<PermissionService>(provider =>
		{
			var database = provider.GetRequiredService<IMongoDatabase>();
			var allowsCollection = database.GetCollection<Allow>("Allows");
			var subjectsCollection = database.GetCollection<Subject>("Subjects");

			return new PermissionService(allowsCollection, subjectsCollection);
		});

		services.AddScoped<MongoDeleteService>(provider =>
		{
			var database = provider.GetRequiredService<IMongoDatabase>();
			return new MongoDeleteService(
				database.GetCollection<Subject>("Subjects"),
				database.GetCollection<Theme>("Themes"),
				database.GetCollection<Resource>("Resources"),
				database.GetCollection<Video>("Videos"),
				database.GetCollection<Test>("Tests"),
				database.GetCollection<Question>("Questions"),
				database.GetCollection<Option>("Options")
			);
		});

		services.AddScoped<MongoPostService>(provider =>
		{
			var database = provider.GetRequiredService<IMongoDatabase>();
			return new MongoPostService(
				database.GetCollection<Option>("Options"),
				database.GetCollection<Question>("Questions"),
				database.GetCollection<Test>("Tests"),
				database.GetCollection<Video>("Videos"),
				database.GetCollection<Theme>("Themes"),
				database.GetCollection<Resource>("Resources"),
				database.GetCollection<Subject>("Subjects")
			);
		});

		return services;
	}
}
