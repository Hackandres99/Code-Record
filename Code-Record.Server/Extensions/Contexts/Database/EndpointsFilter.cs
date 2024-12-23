using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Code_Record.Server.Extensions.Contexts.Database
{
    public class EndpointsFilter : IDocumentFilter
    {
        private readonly string _databaseType;

        public EndpointsFilter(string databaseType)
        {
            _databaseType = databaseType;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = new List<string>();

            foreach (var path in swaggerDoc.Paths)
            {
                if (_databaseType == "MongoAtlas" && path.Key.Contains("/api/sql/"))
                {
                    pathsToRemove.Add(path.Key);
                }

                if (_databaseType == "SQLServer" && path.Key.Contains("/api/mongo/"))
                {
                    pathsToRemove.Add(path.Key);
                }
            }

            foreach (var path in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }
        }
    }
}

