# Web API

A REST API for the products of an e-commerce shop. It lists all products, lists them with pagination (v2), returns one product, creates a product, and updates a product's description. It is built with **.NET 10**, **FastEndpoints**, and **Entity Framework Core**, and stores data in **MS SQL Server**. An in-memory **mock data source** lets the API and its tests run without a database.

## Assignment

| Requirement | Where |
| --- | --- |
| Product with `Id`, `Name`, `ImgUri`, `Price` (required), `Description` (optional) | `Web.Domain/Entities/Product.cs`, `Web.Application/Products/ProductModel.cs`, `Web.Api/Models/ProductDto.cs` |
| List all products | `GET /v1/products` |
| List with pagination, default page size 10, v2 | `GET /v2/products` |
| Get one product by ID | `GET /v1/products/{id}` |
| Update a product's description | `PATCH /v1/products/{id}` |
| Swagger documentation of all endpoints on localhost | `http://localhost:5211/swagger` |
| Unit tests of the endpoints | `tests/` |
| SQL Server storage with seed data, and a mock data source for switching between the two | `DataSource` setting, see [ADR 0005](docs/adr/0005-sql-server-and-entity-framework-core.md) and [ADR 0006](docs/adr/0006-selectable-data-source.md) |

Beyond the assignment, the API has input limits, optimistic concurrency, health checks, rate limiting, and OpenTelemetry. The decisions are in [`docs/adr`](docs/adr/README.md).

## Architecture

| Project | Responsibility |
| --- | --- |
| `Web.Api` | HTTP endpoints, request and response models, request validation (`Validation/`), error responses, Swagger, and the composition root (`Program.cs`). It does not reference `Web.Domain`. |
| `Web.Application` | Use cases (`IProductService`, `ProductService`), the store contract (`IProductRepository`), the facade `ProductValidation` that publishes the domain rules to the HTTP layer, and the error types |
| `Web.Domain` | The `Product` entity and its business rules (`ProductRules`), which the entity checks itself, and the field limits |
| `Web.Infrastructure` | EF Core repository and migrations, the mock repository, seed data, and service registration |
| `Web.AppHost` | .NET Aspire host that starts the API and, in `Database` mode, SQL Server |

The dependencies point inwards, see [ADR 0002](docs/adr/0002-layered-architecture.md). The one exception is that `Web.Api` references `Web.Infrastructure` for the composition root only.

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Docker, only for the **database** data source and for the SQL Server tests

## Running the application

| `DataSource` | Storage | Requirements |
| --- | --- | --- |
| `Database` (default) | MS SQL Server, migrated and seeded on startup | Docker |
| `Mock` (set by the `Development` launch profile) | In memory, pre-filled with sample products. Data is lost when the application stops. | .NET SDK only |

### Option 1: mock data (no database)

```bash
dotnet run --project src/Web.Api
```

Swagger UI is then at **http://localhost:5211/swagger**. The HTTPS profile uses **https://localhost:7252/swagger**.

### Option 2: database through .NET Aspire

1. **Set the SQL Server superuser password** (only once). It must be at least 8 characters and contain characters from three of these sets: uppercase letters, lowercase letters, digits, and symbols. Replace `MyStrongP@ssw0rd` with your password:

   ```bash
   cd src/Web.AppHost
   dotnet user-secrets set "Parameters:sql-password" "MyStrongP@ssw0rd"
   cd ../..
   ```

   You can also set the password in the Aspire dashboard after it starts:

<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/38f509cc-feed-4bfa-8a2d-07343c512f66" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/e8568cd1-63e9-4efb-a3e6-2d5c0b3bc467" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/032b513a-a7e9-40c6-baa1-be89cafc75be" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/1a6d156c-bac3-49f1-b515-2692da007288" />

   Starting the database and applying migrations takes a while. The "unhealthy" indicator is shown until it finishes:

<img width="1419" height="946" alt="image" src="https://github.com/user-attachments/assets/73c2fdf3-6356-49ae-b797-eebd8f6ea90d" />

2. **Start the solution** from the root folder:

   ```bash
   dotnet run --project src/Web.AppHost/Web.AppHost.csproj
   ```

   Aspire starts SQL Server and the API, applies the migrations, and seeds the sample products. The dashboard lists the `web-api` resource, and its link opens Swagger UI. When Docker and the Aspire workload are installed, the database container starts and the API begins to work:

<img width="1419" height="946" alt="image" src="https://github.com/user-attachments/assets/7197c9e7-2be4-4f34-a3ae-fda3d09555e5" />

**Troubleshooting:** SQL Server stores the `sa` password in its data volume when the volume is created. If `sql-password` changes later, the database stays unhealthy with `Login failed for user 'sa'`. Set the original password back, or delete the volume (`web.apphost-<hash>-sql-data`, see `docker volume ls`). Deleting it removes the data.

To run the mock source through Aspire, start the AppHost with `DataSource=Mock`. SQL Server is then not started:

```bash
DataSource=Mock dotnet run --project src/Web.AppHost/Web.AppHost.csproj
```

To run the database without Aspire, set `ConnectionStrings__WebDb` and `DataSource=Database`, and set `ASPNETCORE_ENVIRONMENT=Development` for local development.

## Running the tests

```bash
dotnet test
```

With Docker running, this includes `SqlServerProductRepositoryTests`, which starts SQL Server with Testcontainers. Without Docker, skip that category:

```bash
dotnet test --filter "TestCategory!=Docker"
```

| Project | What it tests |
| --- | --- |
| `Web.Domain.Tests` | The business rules and the `Product` entity |
| `Web.Application.Tests` | The product service, with a mocked store |
| `Web.Api.Tests` | The endpoints through `WebApplicationFactory`, with a mocked service. `MockDataSourceTests` runs the whole pipeline on the mock store. |
| `Web.Infrastructure.Tests` | The EF Core repository (in-memory provider and SQL Server), the mock repository, and the data source registration |

## API

| Method | Route | Version | Description |
| --- | --- | --- | --- |
| `GET` | `/v1/products` | 1 | List all products |
| `GET` | `/v1/products/{id}` | 1 | Get one product |
| `POST` | `/v1/products` | 1 | Create a product |
| `PATCH` | `/v1/products/{id}` | 1 | Update a product's description |
| `GET` | `/v2/products?pageNumber=0&pageSize=10` | 2 | List products with pagination |

Examples are in [`src/Web.Api/Web.Api.http`](src/Web.Api/Web.Api.http).

**Pagination.** `pageNumber` is zero-based and defaults to `0`. `pageSize` defaults to `10` and cannot exceed `100`. The v2 response has `pageNumber`, `pageSize`, `totalCount`, `totalPages`, `hasNext`, and `values`.

**Create and update rules.**

* `name`: not empty, at most 200 characters.
* `price`: greater than 0, at most 2 decimal places, at most `9999999999999999.99`.
* `imgUri`: an absolute `http` or `https` address, at most 2048 characters.
* `description`: optional, at most 4000 characters. A product without one has `"description": ""`.
* `PATCH` requires `description`. `null` is rejected with `400`, and an empty string clears the description.

**Concurrent updates.** If another request saved the product after this request read it, `PATCH` returns `409 Conflict`. Read the product again and retry. See [ADR 0021](docs/adr/0021-optimistic-concurrency-with-row-version.md).

## Errors

Errors use RFC 7807 problem details (`application/problem+json`).

| Status | When |
| --- | --- |
| `400` | Invalid input, malformed JSON, or a value outside the allowed range. The `errors` array lists each field. |
| `404` | The product does not exist |
| `409` | The product was changed by another request after it was read |
| `429` | The client exceeded the rate limit. `Retry-After` gives the wait in seconds. |
| `500` | Unexpected failure. `detail` and `stackTrace` appear only in Development. |

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `DataSource` | `Database` | `Database` or `Mock`. `Mock` is refused in Production. |
| `ConnectionStrings:WebDb` | none | SQL Server connection string, required in `Database` mode. The AppHost provides it in Aspire mode. |
| `Pagination:DefaultPageSize` / `MaxPageSize` | `10` / `100` | Page size defaults and limit. Checked at startup. |
| `RateLimiting:Reads` / `Writes` | 100 / 20 requests per 60 s | Token buckets per client IP for `GET` and for `POST`/`PATCH` |
| `Database:MigrateOnStartup` | `true` | Applies migrations on startup. Set to `false` in Production. |
| `Database:SeedSampleData` | `true` | Adds sample products to an empty table. Set to `false` in Production. |
| `Cors:AllowedOrigins` | none | Browser origins that may call the API |
| `Kestrel:Limits:MaxRequestBodySize` | `65536` | Largest request body, in bytes |

Example, for one run:

```bash
Pagination__DefaultPageSize=20 Pagination__MaxPageSize=50 dotnet run --project src/Web.Api
```

Health checks: `GET /health/live` (the process responds) and `GET /health/ready` (the database accepts connections, `503` otherwise).

## Known limitations

* **`GET /v1/products` has no limit.** It returns every product in one response. Use `GET /v2/products` for large catalogues. The v1 route is kept for the assignment.
* **Conflicts are detected, not merged.** A conflicting `PATCH` gets `409`, and the client must read and retry. The API does not send an `ETag` yet, so a client cannot check the version before it updates ([ADR 0021](docs/adr/0021-optimistic-concurrency-with-row-version.md)).
* **Rate limits are per instance.** Behind a reverse proxy, forwarded headers must be configured first.
* **Page numbers start at 0.** This follows the assignment.

## Migrations

After changing the `Product` entity or `DatabaseContext`, add a migration with the repository's EF Core tool:

```bash
dotnet tool restore
ConnectionStrings__WebDb="Server=localhost;Database=DesignTime;Trusted_Connection=True;TrustServerCertificate=True" dotnet tool run dotnet-ef migrations add <MigrationName> --project src/Web.Infrastructure --startup-project src/Web.Api --output-dir Migrations
```

Applied migrations are never edited. In Production, the API does not migrate on startup. Run the migration step before the new version starts:

```bash
dotnet publish src/Web.Api -c Release -o publish
ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__WebDb="<connection string>" dotnet publish/Web.Api.dll --migrate-only
```

See [`docs/operations.md`](docs/operations.md) for backups, restores, and logs.

## Continuous integration

[`.github/workflows/pr-check.yml`](.github/workflows/pr-check.yml) runs on every pull request into `main`. It checks formatting, builds in Release, fails on known vulnerable packages and on an outdated `THIRD-PARTY-NOTICES.md`, and runs the tests of the projects the change affects. Locally, the same test selection runs with `bash scripts/run-affected-tests.sh origin/main`.

## Documentation

* [`docs/adr`](docs/adr/README.md): architecture decision records
* [`docs/operations.md`](docs/operations.md): running and operating the API
* [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md): licences of the packages that ship with the API

The project is licensed under the [MIT License](LICENSE).
