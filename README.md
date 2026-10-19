# Web API

A REST API for managing the products of an e-commerce shop. It is the implementation of the **WebApi Application** assignment: it lists all products, lists them with pagination (v2), returns one product, creates a product, and updates a product's description. The API is built with **.NET 10**, **FastEndpoints**, and **Entity Framework Core**, and it stores data in **MS SQL Server**. An in-memory **mock data source** lets the API run, and its unit tests run, without a database.

## Assignment coverage

| Requirement from the assignment | Where it is implemented |
| --- | --- |
| Product with `Id`, `Name` (required), `ImgUri` (required), `Price` (required, decimal), `Description` (optional) | `Web.Domain/Entities/Product.cs`, `Web.Api/Models/ProductDto.cs` |
| API endpoint to list all products | `GET /v1/products` |
| v2 of the list endpoint with pagination, default page size 10, versioning shown in the documentation | `GET /v2/products` and the `v2` Swagger document |
| API endpoint to retrieve one product by ID | `GET /v1/products/{id}` |
| API endpoint to update a product's description | `PATCH /v1/products/{id}` |
| Swagger documentation of all endpoints, accessible after running the project on localhost | `http://localhost:5211/swagger` |
| Unit tests covering the endpoints | `tests/Web.Api.Tests` and `tests/Web.Infrastructure.Tests` |
| Layered architecture and SOLID design | `Web.Api`, `Web.Application`, `Web.Domain`, `Web.Infrastructure`; see [ADR 0002](docs/adr/0002-layered-architecture.md) |
| MS SQL Server storage, initial seed data | `Web.Infrastructure` (EF Core, migrations, `DatabaseInitializer`) |
| Mock data for unit tests and development | `Web.Infrastructure/Mock` (`MockProductRepository`, `ProductSeedData`) |
| Easy switch between the mock and the database | the `DataSource` setting, see [ADR 0006](docs/adr/0006-selectable-data-source.md) |
| README with the application, prerequisites, how to run it, and how to run the tests | this file |

## Work beyond the assignment

The assignment asked for the API, its documentation, tests, and a README. The repository also contains the following, so that the project can be developed and released like a production codebase.

* **Local hosting with .NET Aspire.** The AppHost starts the API and SQL Server in a container, with a dashboard for logs, traces, and the API's address. SQL Server data is kept in a Docker volume ([ADR 0007](docs/adr/0007-dotnet-aspire-for-local-hosting.md)).
* **Startup validation and configuration.** Pagination and rate limit settings are validated when the application starts. `Mock` is refused in the Production environment, and a missing connection string stops the start with a clear message.
* **Production behaviour.** Swagger is turned off in Production, security headers are added to every response, and HSTS and a Content-Security-Policy are used. The migrations run as a separate step with `--migrate-only` ([ADR 0018](docs/adr/0018-migrations-as-a-separate-step.md)). An operations guide is in [`docs/operations.md`](docs/operations.md).
* **Rate limiting** per client IP, with separate read and write limits and RFC 7807 responses for rejected requests ([ADR 0015](docs/adr/0015-rate-limiting.md)).
* **Input limits and stable paging.** Field lengths are limited in the validators, the business rules, and the database schema. The product list is ordered by creation time and ID, so paging does not skip or repeat products ([ADR 0019](docs/adr/0019-input-limits-and-paging-order.md)).
* **Health checks** for liveness and readiness, and **OpenTelemetry** traces and metrics ([ADR 0017](docs/adr/0017-operational-endpoints-and-telemetry.md)).
* **Integration tests against a real SQL Server** in a container (Testcontainers), which run the migrations and the repository.
* **Architecture decision records** in [`docs/adr`](docs/adr/README.md), with the reasons for each significant decision, including the accepted risk of anonymous access ([ADR 0016](docs/adr/0016-anonymous-access-for-the-assignment.md)).
* **Code quality tooling:** an `.editorconfig`, code formatting checked with `dotnet format`, the build with warnings treated as errors, and central package management ([ADR 0013](docs/adr/0013-central-package-management.md)).
* **Git hooks** with Husky.Net. Before each commit, the code is formatted and `THIRD-PARTY-NOTICES.md` is regenerated ([see below](#git-hooks)).
* **Semantic versioning** calculated from the git history with GitVersion ([ADR 0020](docs/adr/0020-semantic-versioning-with-gitversion.md)).
* **Continuous integration** on every pull request: formatting, build, a scan for vulnerable packages, a check that the notices are up to date, and the tests of the affected projects ([see below](#continuous-integration)).
* **Licensing.** The project is MIT-licensed. `THIRD-PARTY-NOTICES.md` lists every package that ships with the API, with its licence and copyright notice, and it is generated from the dependencies.
* **Pull request template**.

## Technologies

* .NET 10 and ASP.NET Core, C#
* FastEndpoints: one class per endpoint, validators, versioning, and Swagger per version
* FluentValidation: request validation
* FluentResults: expected failures returned from the repository
* Mapster: mapping of domain objects to DTOs (MIT-licensed)
* Entity Framework Core and MS SQL Server: database storage
* .NET Aspire: local orchestration of the API and SQL Server
* NUnit, Moq, Testcontainers: unit and integration tests
* Husky.Net and GitVersion: git hooks and versioning

Design decisions and their trade-offs are recorded in [`docs/adr`](docs/adr/README.md).

## Architecture

The solution is split into layers. Dependencies point inwards.

| Project | Responsibility |
| --- | --- |
| `Web.Api` | HTTP layer: endpoints (`Features/`), request and response models, validation, error responses, rate limiting, Swagger, and the composition root (`Program.cs`) |
| `Web.Application` | Use cases (`IProductService`, `ProductService`), the business rules (`ProductRules`, `ProductFactory`), the store contract (`IProductRepository`, which only stores and loads products), and the error types |
| `Web.Domain` | The `Product` entity, the field limits (`ProductLimits`), and the image address rules (`ProductImageUri`) |
| `Web.Infrastructure` | Data access: the EF Core repository and migrations, the mock repository, seed data, and service registration |
| `Web.AppHost` | .NET Aspire host that starts the API and, in the `Database` mode, SQL Server |

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Docker, only for the **database** data source (SQL Server runs in a container through .NET Aspire)
* Git, for the git hooks

## Running the application

The data source is set by the `DataSource` setting:

| Value | Storage | Requirements |
| --- | --- | --- |
| `Mock` (default) | In memory, pre-filled with sample products. Data is lost when the application stops. | .NET SDK only |
| `Database` | MS SQL Server, migrated and seeded on startup | Docker |

### Option 1: mock data (no database)

Run the API project:

```bash
dotnet run --project src/Web.Api
```

Swagger UI is then available at **http://localhost:5211/swagger**. The HTTPS profile of the launch settings uses **https://localhost:7252/swagger**.

In the IDE, run the `http` profile of `Web.Api`. It uses the mock data source, because `appsettings.Development.json` sets `DataSource` to `Mock`, and the browser opens Swagger. The `DataSource` value of the AppHost overrides this setting, so Option 2 still uses the database.

### Option 2: database through .NET Aspire

1. **Set the SQL Server superuser password** (only needed once). The password must be at least 8 characters long and contain characters from three of the following four sets: uppercase letters, lowercase letters, digits, and symbols. Replace `MyStrongP@ssw0rd` with your password:

   ```bash
   cd src/Web.AppHost
   dotnet user-secrets set "Parameters:sql-password" "MyStrongP@ssw0rd"
   cd ../..
   ```
There is also the option to set a password via the UI after launching .NET Aspire:

<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/38f509cc-feed-4bfa-8a2d-07343c512f66" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/e8568cd1-63e9-4efb-a3e6-2d5c0b3bc467" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/032b513a-a7e9-40c6-baa1-be89cafc75be" />
<img width="2838" height="1892" alt="image" src="https://github.com/user-attachments/assets/1a6d156c-bac3-49f1-b515-2692da007288" />

Starting the database and applying migrations may take some time. While the process is underway, the “unhealthy” indicator will be visible.
<img width="1419" height="946" alt="image" src="https://github.com/user-attachments/assets/73c2fdf3-6356-49ae-b797-eebd8f6ea90d" />

If Docker and Aspire Workload are installed, the database container will start up, and the API will begin working:
<img width="1419" height="946" alt="image" src="https://github.com/user-attachments/assets/7197c9e7-2be4-4f34-a3ae-fda3d09555e5" />




2. **Start the solution** from the root folder of this project:

   ```bash
   dotnet run --project src/Web.AppHost/Web.AppHost.csproj
   ```

   Aspire starts SQL Server and the API, applies the EF Core migrations, and seeds the sample products. The Aspire dashboard (its URL is printed in the console) lists the `web-api` resource. The link of the resource opens Swagger UI.

**Troubleshooting:** SQL Server stores the `sa` password in its data volume when the volume is first created. If `sql-password` is changed later, the database stays unhealthy and the API does not start, with `Login failed for user 'sa'` in the SQL Server log. Either set the password back to the original value, or remove the volume and start again. The volume is named `web.apphost-<hash>-sql-data` (see `docker volume ls`). Removing it deletes the data.

To run the **mock** source through Aspire, start the AppHost with `DataSource=Mock`. SQL Server is then not started and the password is not needed:

```bash
DataSource=Mock dotnet run --project src/Web.AppHost/Web.AppHost.csproj
```

To run the **database** source without Aspire, set the connection string `ConnectionStrings__WebDb` (environment variable) and run the API with `DataSource=Database`. ASP.NET Core uses the `Production` environment unless `ASPNETCORE_ENVIRONMENT` is set, so set `ASPNETCORE_ENVIRONMENT=Development` for local development.

## Running the tests

The tests need the .NET 10 SDK. One group of tests also needs **Docker**: `SqlServerProductRepositoryTests` starts a real SQL Server container with Testcontainers, and these tests are in the `Docker` category.

With Docker running, run all tests:

```bash
dotnet test
```

Without Docker, skip the tests of the `Docker` category:

```bash
dotnet test --filter "TestCategory!=Docker"
```

The rest of the tests do not need Docker or a database: `Web.Application.Tests` tests the business rules and the product service, `Web.Api.Tests` calls the endpoints through `WebApplicationFactory` with a mocked service, and `Web.Infrastructure.Tests` tests the EF Core repository (EF Core in-memory provider), the mock repository, and the data source registration.

## API endpoints

| Method | Route | Version | Description |
| --- | --- | --- | --- |
| `GET` | `/v1/products` | 1 | List all products |
| `GET` | `/v1/products/{id}` | 1 | Get one product by ID |
| `POST` | `/v1/products` | 1 | Create a product |
| `PATCH` | `/v1/products/{id}` | 1 | Update a product description |
| `GET` | `/v2/products?pageNumber=0&pageSize=10` | 2 | List products with pagination |

Examples are in [`src/Web.Api/Web.Api.http`](src/Web.Api/Web.Api.http).

**Pagination:** `pageNumber` is zero-based and optional, and it defaults to `0`. `pageSize` is optional, defaults to the configured default (`10`), and cannot exceed the configured maximum (`100`). To get all products, use `GET /v1/products`.

**Create request:**

```json
{
  "name": "Phone",
  "imgUri": "https://example.com/phone.jpg",
  "price": 100,
  "description": "A sample product."
}
```

`name`, `imgUri`, and `price` are required:

* `name` is not empty and has at most 200 characters.
* `price` is greater than 0, has at most 2 decimal places, and is at most `9999999999999999.99` (the largest value that the `decimal(18,2)` column stores).
* `imgUri` is an absolute `http` or `https` address with at most 2048 characters. Other schemes, such as `javascript:` or `file:`, are rejected.
* `description` is optional, has at most 4000 characters, and may be omitted or `null`. A product created without a description returns `"description": ""`.

**Updating the description:** `PATCH /v1/products/{id}` requires the `description` field. A `null` value is rejected with `400`, and an empty string clears the description.

**Product response:** the dates are UTC with an explicit offset, for example `"createdAt": "2026-10-19T11:45:44.771147+00:00"`. The same format is used with the mock and the database data sources.

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

Every problem response has the same members: `type`, `title`, `status`, `detail`, `instance`, and `traceId`. The `errors` array is present for validation errors.

Example validation error:

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The price must be greater than 0.",
  "instance": "/v1/products",
  "traceId": "0HNPE5V0LFNQ6:00000001",
  "errors": [
    {
      "name": "price",
      "reason": "The price must be greater than 0.",
      "code": "ERROR_0003"
    }
  ]
}
```

## Project structure

```
Web.slnx
src/
  Web.Api/                 HTTP endpoints, models, validation, rate limiting, Swagger
  Web.Application/         Use cases, business rules, repository contract, commands, queries
  Web.Domain/              Product entity, field limits, image address rules
  Web.Infrastructure/      EF Core, migrations, mock repository, seed data
  Web.AppHost/             .NET Aspire host (API + SQL Server)
tests/
  Web.Api.Tests/
  Web.Application.Tests/
  Web.Infrastructure.Tests/
docs/adr/                  Architecture decision records
docs/operations.md         Running and operating the API
scripts/                   Setup, notice generation, and the pre-commit script
.husky/                    Git hooks
THIRD-PARTY-NOTICES.md     Licences of the third-party packages in the API
GitVersion.yml             Versioning settings
```

## Git hooks

The git hooks are managed by [Husky.Net](https://alirezanet.github.io/Husky.Net/). The `setup` script installs them. To install them by hand, run `dotnet tool restore` and then `dotnet husky install` once after cloning the repository.

The **pre-commit** hook runs `scripts/pre-commit.sh` before each commit. It:

1. formats the solution with `dotnet format whitespace`;
2. regenerates `THIRD-PARTY-NOTICES.md`;
3. stages the files that were already staged, and the regenerated notices, so that the commit contains the formatted code.

Files that were not staged are formatted in the working tree but are not added. On Windows, the hook needs Git Bash, which is installed with Git for Windows.

## Versioning

The version is calculated from the git history with GitVersion. The settings are in `GitVersion.yml`, and the tool is pinned in `.config/dotnet-tools.json`:

```bash
dotnet tool run dotnet-gitversion /showvariable FullSemVer
```

The first release is `1.0.0`. Commits on `main` are patch increments, and feature branches get the `alpha` label. A maintainer creates the release tag `vX.Y.Z` on `main`. See [ADR 0020](docs/adr/0020-semantic-versioning-with-gitversion.md).

## Third-party notices

`THIRD-PARTY-NOTICES.md` lists every package that ships with the API, with its licence and copyright notice. It is regenerated by the pre-commit hook, and it can be regenerated by hand:

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

* **quality** calculates the version, checks formatting (`dotnet format whitespace`), builds with warnings as errors, fails when a package has a known vulnerability, and fails when `THIRD-PARTY-NOTICES.md` is out of date.
* **affected-tests** finds the projects affected by the pull request with [dotnet-affected](https://github.com/leonardochaia/dotnet-affected), builds them, and runs the tests of the affected test projects. Unaffected tests are skipped, so a change to one part of the solution does not rerun everything.

The workflow does not deploy anything.

To run the same test selection locally against `main`:

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

Existing migrations are never edited after they are applied. A schema change is always a new migration.

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

This project is designed as a foundation for a modular e-commerce API. The following steps are planned, and each one is described in the ADR it relates to:

* **Authentication and authorization** for the write endpoints, then for the read endpoints ([ADR 0016](docs/adr/0016-anonymous-access-for-the-assignment.md)).
* **Trusted forwarded headers** and a shared rate limit store, so that the limits apply per client and across instances ([ADR 0015](docs/adr/0015-rate-limiting.md)).
* **Keyset pagination** if the product catalogue grows large enough that offset paging becomes slow ([ADR 0010](docs/adr/0010-pagination.md)).
* **Compile-time mapping** with Riok.Mapperly, if mapping becomes a measured cost ([ADR 0011](docs/adr/0011-mapster-for-object-mapping.md)).
