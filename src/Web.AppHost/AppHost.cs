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

    IResourceBuilder<SqlServerDatabaseResource> sql = builder
        .AddSqlServer("sql", saPassword, 1433)
        .WithImageTag("2022-latest")
        .WithBindMount("./sql-data", "/var/opt/mssql")
        .AddDatabase("WebDb");

    api.WaitFor(sql).WithReference(sql);
}

await builder.Build().RunAsync();
