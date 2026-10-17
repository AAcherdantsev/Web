using FastEndpoints.Swagger;
using NJsonSchema.Generation;

namespace Web.Api.Extensions;

/// <summary>
///     Provides extension methods for configuring Swagger documents in the service collection.
/// </summary>
internal static class SwaggerDocumentExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds Swagger documents to the service collection for each specified version.
        /// </summary>
        public  void AddSwaggerDocuments()
        {
            services.AddSwaggerDocument(1);
            services.AddSwaggerDocument(2);
        }

        /// <summary>
        ///     Configures and adds a Swagger document to the service collection for the specified API version.
        /// </summary>
        /// <param name="version">The API version for which the Swagger document is being generated.</param>
        private void AddSwaggerDocument(int version)
        {
            services.SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.DocumentName = "v" + version;
                    s.Title = "Web API V" + version;
                    s.Version = "v" + version;
                    s.SchemaSettings.SchemaNameGenerator = new DefaultSchemaNameGenerator();
                };
                o.MaxEndpointVersion = version;
                o.MinEndpointVersion = version;
            });
        }
    }
}
