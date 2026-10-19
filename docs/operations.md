# Operations

This guide covers running the Web API in a production-like environment: configuration, database migrations, health probes, backups, and logs. It is written for the people who deploy and run the service. The repository does not contain a container image or deployment automation. The API is published with `dotnet publish`, and the target platform decides how it is hosted.

## Publishing

Publish the API from the root folder of this project:

```bash
dotnet publish src/Web.Api -c Release -o publish
```

The output folder contains `Web.Api.dll`, which runs with `dotnet publish/Web.Api.dll`. Use the same output for the migration step and for the running service.

## Configuration

Settings are read from `appsettings.json`, then `appsettings.<Environment>.json`, then environment variables. Use environment variables for anything that differs between environments, and never put secrets in the appsettings files.

| Variable | Required | Default | Purpose |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Production` when not set | `Production` disables Swagger, adds the Content-Security-Policy header and HSTS, and refuses the `Mock` data source. |
| `ConnectionStrings__WebDb` | Yes, in `Database` mode | none | SQL Server connection string. Store it in the platform's secret store. |
| `DataSource` | No | `Database` | `Database` or `Mock`. `Mock` is for local development only and is refused in Production. |
| `Database__MigrateOnStartup` | No | `false` in `appsettings.Production.json` | Applies migrations on startup. Keep it `false` in Production and run the migration step instead (see below). |
| `Database__SeedSampleData` | No | `false` in `appsettings.Production.json` | Adds sample products to an empty table. Keep it off for real data. |
| `Cors__AllowedOrigins__0` | No | none | Origins that may call the API from a browser, for example `https://shop.example.com`. Without a value, no cross-origin browser requests are allowed. |
| `Pagination__DefaultPageSize` / `Pagination__MaxPageSize` | No | `10` / `100` | Page size defaults and limit. |
| `RateLimiting__Reads__*` / `RateLimiting__Writes__*` | No | see the README | Rate limits per client IP. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Recommended | `http://localhost:4317` | Where traces and metrics are sent. |
| `Kestrel__Limits__MaxRequestBodySize` | No | `65536` | Largest accepted request body, in bytes. |

The application refuses to start, and says why, when a required setting is missing or invalid. For example, `Mock` in Production or a missing `WebDb` connection string stops the process at startup.

## Database migrations

Migrations are a separate deployment step. Do not let every instance migrate on startup.

1. Run the migration step once per release, before starting the new version:

   ```bash
   ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__WebDb="<connection string>" dotnet publish/Web.Api.dll --migrate-only
   ```

   The command applies the pending migrations and exits with code `0`. A failure exits with a non-zero code, and the new version must not be started.

2. Start the new version of the API. It does not migrate on startup in Production.

Rolling back a migration is not automatic. Restore the database from a backup taken before the migration (see below), then deploy the previous version of the published output. Plan this for every release that changes the schema.

## Health probes

| Endpoint | Meaning | Use for |
| --- | --- | --- |
| `GET /health/live` | The process is running and responds. Does not check dependencies. | Liveness probe. Restart the instance when it fails. |
| `GET /health/ready` | The process is running and the database accepts connections. Returns `503` otherwise. | Readiness probe. Stop routing traffic to the instance when it fails. |

With the `Mock` data source, which is refused in Production, the readiness probe always succeeds.

## Logs and telemetry

* **Logs** are written to the console in a plain format, one line per event. The platform collects the console output. Exceptions are logged with their stack traces.
* **Traces and metrics** are sent with OpenTelemetry to the endpoint in `OTEL_EXPORTER_OTLP_ENDPOINT`. The service name is `web-api`. ASP.NET Core requests, SQL Server calls, and runtime metrics are included.
* Logs do not include request bodies or connection strings. In Production, Entity Framework Core logs its SQL commands only as warnings, so the statements and their parameter values are not written to the logs by default.

## Backups and restore

The product data lives in one SQL Server database, `WebDb`, in the `Products` table. The procedure below uses SQL Server's native backup. Adapt it to the managed backup feature of the hosting platform when one exists.

Full backup, once a day, with a retention of at least 14 days:

```sql
BACKUP DATABASE [WebDb]
TO DISK = N'/var/opt/mssql/backup/WebDb-full.bak'
WITH COMPRESSION, CHECKSUM, INIT;
```

Take a backup before every migration step, with a different file name, so that a rollback is possible.

Restore:

```sql
ALTER DATABASE [WebDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [WebDb]
FROM DISK = N'/var/opt/mssql/backup/WebDb-full.bak'
WITH REPLACE, CHECKSUM;
ALTER DATABASE [WebDb] SET MULTI_USER;
```

Test the restore regularly on a separate server. A backup that has never been restored is not known to work. Record the date of the last successful test restore.

## Security checklist for a deployment

* `ASPNETCORE_ENVIRONMENT=Production`, so Swagger is off and the security headers are on.
* The connection string comes from the secret store, not from a file in the published output or the repository.
* `Cors__AllowedOrigins` lists only the front-end origins that are needed.
* Forwarded headers are not configured. Behind a reverse proxy, the rate limiter sees the proxy's address for every client, so the limits apply to all clients together.
* The database user has only the rights the API needs: read and write on `Products`, plus rights to run migrations when the migration step uses the same user.
* Authentication is not implemented. The endpoints are anonymous, which is accepted for this assignment (see [ADR 0016](adr/0016-anonymous-access-for-the-assignment.md)). Do not expose the service to untrusted networks without adding it.
