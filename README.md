# Web API

**Web API** is a REST API for managing the products of a demo e-commerce shop. It is built with **.NET 10**, **FastEndpoints**, and **Entity Framework Core**, and it stores data in **MS SQL Server**. The solution also has an in-memory **mock data source**, so the API can run without a database.

## Features

* Products with the attributes `Id`, `Name`, `ImgUri`, `Price`, and `Description` (optional)
* Endpoints (see [API endpoints](#api-endpoints)):
  * list all products (v1)
  * list products with pagination, default page size 10 (v2)
  * get a product by ID (v1)
  * create a product (v1)
  * update a product description (v1)
* API versioning through the URL (`/v1/...`, `/v2/...`), reflected in the Swagger documents
* Swagger UI for all endpoints
* Switching between the **mock** and **database** data sources with one setting
* Database migrations and seed data applied automatically on startup in development
* Unit and integration tests

## Technologies

* .NET 10 + ASP.NET Core
* FastEndpoints — endpoint-per-class API design with validators and versioning
* FluentValidation — request validation
* FluentResults — expected failures returned from the repository
* Mapster — DTO ↔ domain mapping (MIT-licensed)
* Entity Framework Core + MS SQL Server — database storage
* .NET Aspire — starts SQL Server in Docker for the database mode
* NUnit, Moq, Testcontainers — unit and integration tests

The main design decisions and their trade-offs are recorded in [`docs/adr`](docs/adr/README.md). Operating the API is described in [`docs/operations.md`](docs/operations.md).

## Architecture

The solution is split into layers. Dependencies point inwards.

| Project | Responsibility |
| --- | --- |
| `Web.Api` | HTTP layer: endpoints (`Features/`), request and response models, validation, error responses, rate limiting, Swagger, and the composition root (`Program.cs`) |
| `Web.Application` | Use-case contracts: `IProductRepository`, commands and queries (`CreateProductCommand`, `GetPagedProductsQuery`), `ProductFactory` with business rules, and the error types |
| `Web.Domain` | The `Product` entity and the `ProductLimits` constants |
| `Web.Infrastructure` | Data access: the EF Core repository and migrations, the in-memory mock repository, seed data, and service registration |
| `Web.AppHost` | .NET Aspire host that starts the API and, in the database mode, SQL Server |

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Docker (only for the **database** data source, which starts SQL Server through .NET Aspire)

To install the prerequisites, you can use the `setup` script from the **scripts** folder. Run it from the root folder of this project:

```bash
bash scripts/setup.sh            # Linux/macOS
```

```powershell
.\scripts\setup.ps1              # Windows
```

## Running the application

The data source is set by the `DataSource` setting:

| Value | Storage | Requirements |
| --- | --- | --- |
| `Mock` | In-memory, pre-filled with sample products. Data is lost when the application stops. | .NET SDK only |
| `Database` (default) | MS SQL Server, migrated and seeded on startup | Docker and the SQL password (see below) |

### Option 1: mock data (no database)

Run the API project directly with `DataSource` set to `Mock`:

```bash
DataSource=Mock dotnet run --project src/Web.Api
```

With the `http` launch profile, Swagger UI is then available at **http://localhost:5211/swagger**.

### Option 2: database through .NET Aspire

1. **Set the SQL Server superuser password** (only needed once). The password must be at least 8 characters long and contain characters from three of the following four sets: uppercase letters, lowercase letters, digits, and symbols. Replace `MyStrongP@ssw0rd` with your password:

   ```bash
   cd src/Web.AppHost
   dotnet user-secrets set "Parameters:sql-password" "MyStrongP@ssw0rd"
   cd ../..
   ```

2. **Start the solution** from the root folder of this project:

   ```bash
   dotnet run --project src/Web.AppHost/Web.AppHost.csproj
   ```

   Aspire starts SQL Server and the API, applies the EF Core migrations, and seeds the sample products. The Aspire dashboard lists the `web-api` resource; its link opens Swagger UI.

To run the **database** source without Aspire, set the connection string `ConnectionStrings__WebDb` (environment variable) and run the API with `DataSource=Database`. ASP.NET Core uses the `Production` environment unless `ASPNETCORE_ENVIRONMENT` is set, so set `ASPNETCORE_ENVIRONMENT=Development` for local development.

To run the **mock** source through Aspire, start the AppHost with `DataSource=Mock`. SQL Server is then not started and the password is not needed:

```bash
DataSource=Mock dotnet run --project src/Web.AppHost/Web.AppHost.csproj
```

### Option 3: Docker image

The `Dockerfile` runs the Aspire AppHost in a container. The AppHost starts the API and, in the `Database` mode, SQL Server, so the whole environment comes up with one command. Docker is needed on the host.

Build the image from the root folder of this project:

```bash
docker build -t web-api .
```

Run it. The AppHost needs the Docker socket to start the SQL Server container, and the SQL password is passed as an environment variable:

```bash
docker run --rm -it \
  -p 8080:8080 -p 18888:18888 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --add-host=host.docker.internal:host-gateway \
  -e "Parameters__sql-password=<password>" \
  web-api
```

* The API is at `http://localhost:8080`, and Swagger UI is at `http://localhost:8080/swagger`. If port 8080 is taken on the host, change the first number of `-p`.
* The Aspire dashboard is at `http://localhost:18888`. The login link is printed in the console.
* The SQL Server data is kept in a Docker volume that Aspire creates, not in the `sql-data` folder.
* To run the mock source, add `-e DataSource=Mock`. No SQL Server container is started then, and the password is not needed.

> **Security:** The Docker socket gives the container control of the Docker host, so run this image only on a machine you trust. The image runs the API in the Development environment: Swagger is enabled and the endpoints are anonymous (see [ADR 0016](docs/adr/0016-anonymous-access-for-the-assignment.md)). Do not publish its ports to untrusted networks.

The image is for local runs and the assignment. It contains the SDK and the source, so it is not a production image. It was tested with Docker Desktop on macOS. On Linux, `host.docker.internal` is provided by the `--add-host` option above, but this has not been tested.

## Unit and integration tests

From the root folder of this project:

```bash
dotnet test
```

Most tests do not need a database or Docker:

* `Web.Api.Tests` calls the endpoints through `WebApplicationFactory` with a mocked repository, and checks validation, error responses, rate limits, health checks, and security headers.
* `Web.Infrastructure.Tests` tests the EF Core repository (EF Core in-memory provider), the mock repository, and the data source registration.
* `SqlServerProductRepositoryTests` runs the migrations and the repository against a real SQL Server in a container. It needs a running Docker daemon.

## API endpoints

| Method | Route | Version | Description |
| --- | --- | --- | --- |
| `GET` | `/v1/products` | 1 | List all products |
| `GET` | `/v1/products/{id}` | 1 | Get one product by ID |
| `POST` | `/v1/products` | 1 | Create a product |
| `PATCH` | `/v1/products/{id}` | 1 | Update a product description |
| `GET` | `/v2/products?pageNumber=0&pageSize=10` | 2 | List products with pagination |

Examples are in [`src/Web.Api/Web.Api.http`](src/Web.Api/Web.Api.http).

**Pagination:** `pageNumber` is zero-based and defaults to `0`. `pageSize` defaults to the configured default (`10`) and cannot exceed the configured maximum (`100`). To get all products, use `GET /v1/products`.

**Create request:**

```json
{
  "name": "Phone",
  "imgUri": "https://example.com/phone.jpg",
  "price": 100,
  "description": "A sample product."
}
```

`name`, `imgUri`, and a `price` greater than 0 are required. `description` is optional.

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `DataSource` | `Database` | `Database` or `Mock`. Set in `appsettings.json`, as an environment variable, or on the command line. |
| `Pagination:DefaultPageSize` | `10` | Page size used when a v2 request omits `pageSize`. |
| `Pagination:MaxPageSize` | `100` | Largest page size a client may request. |
| `RateLimiting:Reads` | 100 per 60 s, burst 100 | Token bucket for `GET` requests, per client IP. |
| `RateLimiting:Writes` | 20 per 60 s, burst 20 | Token bucket for `POST` and `PATCH` requests, per client IP. |
| `ASPNETCORE_ENVIRONMENT` | `Development` (launch profile) | `Production` turns Swagger off, adds the Content-Security-Policy and HSTS, and refuses the `Mock` data source. |
| `Database:MigrateOnStartup` | `true` | Applies migrations on startup. Set to `false` in Production and use `--migrate-only` instead. |
| `Database:SeedSampleData` | `true` | Adds sample products to an empty table. Set to `false` in Production. |
| `Cors:AllowedOrigins` | none | Browser origins that may call the API. Empty means no cross-origin browser requests. |
| `Kestrel:Limits:MaxRequestBodySize` | `65536` | Largest accepted request body, in bytes. |
| `ConnectionStrings:WebDb` (`ConnectionStrings__WebDb`) | — | SQL Server connection string, required in `Database` mode. Provided by the AppHost in Aspire mode. |

The pagination and rate limit settings are checked at startup. An invalid value (for example, a default larger than the maximum) stops the application with a clear error.

Example, to change the defaults for one run:

```bash
Pagination__DefaultPageSize=20 Pagination__MaxPageSize=50 dotnet run --project src/Web.Api
```

## Health checks

| Endpoint | Meaning |
| --- | --- |
| `GET /health/live` | The process responds. Use it for the liveness probe. |
| `GET /health/ready` | The process responds and the database accepts connections. Returns `503` otherwise. Use it for the readiness probe. |

Telemetry (traces and metrics) is exported with OpenTelemetry to `OTEL_EXPORTER_OTLP_ENDPOINT`. Logs go to the console.

## Rate limiting

Each client (identified by IP address) has two token buckets: one for reads and one for writes. A bucket holds up to `TokenLimit` tokens, so a client can burst that many requests at once, and it refills at `TokensPerPeriod` tokens per `ReplenishmentPeriodSeconds`. A request without a token is rejected with `429 Too Many Requests` and is not queued. The algorithm and the reasons for it are in [ADR 0015](docs/adr/0015-rate-limiting.md).

Limits are per instance. Behind a reverse proxy, the limiter sees the proxy's address for every client, so forwarded headers must be configured before the limits can be relied on. This is not configured yet.

## REST conventions

* Resources are plural nouns in the URL. The version is the first path segment (`/v1`, `/v2`).
* `GET` reads, `POST` creates, `PATCH` changes part of a resource. Nothing else is exposed.
* `POST` returns `201 Created` and a `Location` header with the URL of the new product.
* Errors use RFC 7807 problem details with the `application/problem+json` content type.

## Errors

| Status | When | Body |
| --- | --- | --- |
| `400` | Invalid input, malformed JSON, or a value outside the allowed range | Problem details with an `errors` array of `{ "name", "reason" }` |
| `404` | The product does not exist | Problem details with `title` and `detail` |
| `429` | The client exceeded the rate limit | Problem details. The `Retry-After` header gives the number of seconds to wait. |
| `500` | Unexpected failure | Problem details with a generic `title`. `detail` and `stackTrace` appear only in the Development environment. |

Example validation error:

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": [
    { "name": "price", "reason": "The price must be greater than 0." }
  ]
}
```

## Project structure

```
Web.slnx
src/
  Web.Api/                 HTTP endpoints, models, validation, rate limiting, Swagger
  Web.Application/         Repository contract, commands, queries, business rules
  Web.Domain/              Product entity and limits
  Web.Infrastructure/      EF Core, migrations, mock repository, seed data
  Web.AppHost/             .NET Aspire host (API + SQL Server)
tests/
  Web.Api.Tests/
  Web.Infrastructure.Tests/
docs/adr/                  Architecture decision records
docs/operations.md         Running and operating the API
scripts/                   Environment setup and notice generation scripts
THIRD-PARTY-NOTICES.md     Licences of the third-party packages in the API
```

## Third-party notices

`THIRD-PARTY-NOTICES.md` lists every package that ships with the API, with its licence and copyright notice. Regenerate it after changing packages:

```bash
bash scripts/generate-third-party-notices.sh
```

The script reads the runtime dependency tree of `Web.Api`, so the file reflects what is actually deployed.

## Managing NuGet packages

Versions are managed centrally in `Directory.Packages.props`, so each version is defined in one place:

1. Add the package and its version to `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="Some.Package" Version="1.2.3" />
   ```
2. Reference it in a project without a version:
   ```xml
   <PackageReference Include="Some.Package" />
   ```

Transitive packages can be pinned the same way. `Directory.Build.props` applies shared settings to all projects.

## Continuous integration

The workflow `.github/workflows/pr-check.yml` runs on every pull request into `main` and has two jobs:

* **quality** checks formatting (`dotnet format whitespace`), builds with warnings as errors, fails when a package has a known vulnerability, and fails when `THIRD-PARTY-NOTICES.md` is out of date.
* **affected-tests** finds the projects affected by the pull request with [dotnet-affected](https://github.com/leonardochaia/dotnet-affected), builds them, and runs the tests of the affected test projects. Unaffected tests are skipped, so a change to one part of the solution does not rerun everything.

The workflow does not deploy anything.

To run the same check locally against `main`:

```bash
bash scripts/run-affected-tests.sh origin/main
```

Set `UNCOMMITTED=All` to include uncommitted changes in the working tree.

Pull requests use the template in `.github/pull_request_template.md`.

## Adding a database migration

The EF Core tools are a local tool of this repository (see `.config/dotnet-tools.json`). After changing the `Product` entity or `DatabaseContext`, run:

```bash
dotnet tool restore
ConnectionStrings__WebDb="Server=localhost;Database=DesignTime;Trusted_Connection=True;TrustServerCertificate=True" dotnet tool run dotnet-ef migrations add <MigrationName> --project src/Web.Infrastructure --startup-project src/Web.Api --output-dir Migrations
```

## Applying migrations in production

In Production, the API does not migrate on startup. Run the migration step once per release, before the new version starts. It applies the pending migrations and exits with code `0`, or with a non-zero code on failure:

```bash
dotnet publish src/Web.Api -c Release -o publish
ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__WebDb="<connection string>" dotnet publish/Web.Api.dll --migrate-only
```

See [`docs/operations.md`](docs/operations.md) for the full procedure, backups, and restores. The decision is recorded in [ADR 0018](docs/adr/0018-migrations-as-a-separate-step.md).

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE). Third-party packages are licensed under their own terms, which are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Future Roadmap

This project is designed as a foundation for a modular e-commerce API.
In the future, the following improvements and features can be implemented:
* User authentication and authorization — add JWT-based authentication
