using Microsoft.OpenApi.Models;
using MongoDB.Bson;

namespace Code_Record.Server.Extensions.Contexts.Database
{
    public static class Type
    {
        public static void DatabaseType(this WebApplicationBuilder builder)
        {
            var databaseType = builder.Configuration["DatabaseType"];
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName?.Replace('.', '_'));
                options.MapType<ObjectId>(() => new OpenApiSchema { Type = "string" });
                options.DocumentFilter<EndpointsFilter>(databaseType);
            });
            if (databaseType == "MongoAtlas")
                builder.Services.AddMongoAtlasContext(builder.Configuration);
            else if (databaseType == "SQLServer")
                builder.Services.AddSQLServerContext(builder.Configuration);
        }
    }
}
