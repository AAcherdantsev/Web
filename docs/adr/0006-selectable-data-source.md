# 0006. Selectable data source (Database or Mock)

Date: 2026-10-16
Status: Accepted

## Context

The task requires that it "shall be easy to switch between the mock and DB data". Developers should be able to run and test the API without Docker, and the database must remain the default for the real deployment.

## Decision

The `DataSource` setting selects the repository implementation:

- `Database` (default): `ProductRepository` on SQL Server, with migrations and seed data applied on startup.
- `Mock`: `MockProductRepository`, an in-memory implementation seeded with the same sample products (`ProductSeedData`). No database is created.

The value comes from configuration, so it can be set in `appsettings.json`, an environment variable, or the command line. The AppHost reads the same setting and starts SQL Server only in `Database` mode. An unknown value fails at startup with a clear message.

Both repositories are used through the same application service, which applies the business rules (see [0002](0002-layered-architecture.md)), so they behave the same way.

## Consequences

- The same endpoints and tests work with both sources.
- The mock is not persistent. Data is lost when the process stops, and it is only suitable for development and tests.
- Two implementations must be kept in step. Each has its own tests in `Web.Infrastructure.Tests`, and a change to the contract must update both.
- The default is the database, so forgetting the setting in production does not silently use mock data.

## Alternatives considered

- **EF Core in-memory provider as the mock.** Less code, but it is not a real database and behaves differently from SQL Server in some queries. A dedicated repository makes the mock's behaviour explicit.
- **Separate projects for each data source.** More structure than this scope needs.
