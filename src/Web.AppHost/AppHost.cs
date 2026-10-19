using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// "Database" starts SQL Server in the container. "Mock" runs the API with in-memory data and needs no Docker.
string dataSource = builder.Configuration["DataSource"] ?? "Database";

IResourceBuilder<ProjectResource> api = builder.AddProject<Web_Api>("web-api")
    .WithEnvironment("DataSource", dataSource)
    // The API has no endpoint at the root, so the dashboard link opens Swagger UI instead
    .WithUrlForEndpoint("http", url => url.Url = "/swagger");

if (dataSource == "Database")
{
    IResourceBuilder<ParameterResource> saPassword = builder.AddParameter("sql-password", true);

    // The data lives in a Docker volume managed by Aspire. A bind mount would need the folder to be writable by the
    // SQL Server user (uid 10001) on every host, which is not the case on Linux.
    IResourceBuilder<SqlServerDatabaseResource> sql = builder
        .AddSqlServer("sql", saPassword)
        .WithImageTag("2022-latest")
        .WithDataVolume()
        .AddDatabase("WebDb");

    api.WaitFor(sql).WithReference(sql);
}

await builder.Build().RunAsync();
