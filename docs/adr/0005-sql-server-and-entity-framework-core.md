# 0005. Store data in SQL Server with Entity Framework Core

Date: 2026-10-16
Status: Accepted

## Context

The task prefers MS SQL Server for product storage and allows any ORM. The schema is small and changes rarely.

## Decision

- Product data is stored in **SQL Server 2022** (started by .NET Aspire in a container, see [0007](0007-dotnet-aspire-for-local-hosting.md)).
- Access uses **EF Core 10** with code-first migrations. Migrations live in `Web.Infrastructure/Migrations`. They are applied on startup in development and by a separate step in Production (see [0018](0018-migrations-as-a-separate-step.md)).
- Prices are `decimal(18,2)`. The precision is set explicitly in `DatabaseContext`, because EF Core otherwise uses a default that silently truncates values.
- Existing migrations are never edited after they are applied. Schema changes are made with a new migration.

## Consequences

- Schema changes are reviewable SQL, and the same migrations run in every environment.
- In development, migrations are applied on startup, so a failing migration stops the application before it serves traffic. In Production they run as a separate step (see [0018](0018-migrations-as-a-separate-step.md)).
- Database mode needs Docker. The mock data source (see [0006](0006-selectable-data-source.md)) covers development without Docker.
- EF Core's `Design` package is referenced by the API project so that `dotnet ef` can run. It ships in the API output, which we accept for now. A separate design-time project would remove it.

## Alternatives considered

- **PostgreSQL.** A good option, but the task prefers SQL Server.
- **Dapper.** Gives more control over SQL, at the cost of writing and maintaining the SQL and the migrations ourselves. Not needed for this scope.
