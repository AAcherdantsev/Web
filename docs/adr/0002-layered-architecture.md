# 0002. Layered architecture with a single dependency direction

Date: 2026-10-16
Status: Accepted

## Context

The task requires a layered architecture and SOLID design. The HTTP framework, the database, and the business rules change for different reasons, so they should not be tangled together.

## Decision

The solution has four source projects with these dependencies:

- `Web.Domain` depends on nothing. It contains the `Product` entity and the `ProductLimits` constants.
- `Web.Application` depends on Domain. It defines the repository contract (`IProductRepository`), the commands and queries, the `ProductFactory` business rules, and the error types.
- `Web.Infrastructure` depends on Application and Domain. It implements the repository with EF Core, provides the in-memory mock, and registers services.
- `Web.Api` depends on Application and Domain. It also references Infrastructure, but only for the composition root: `Program.cs` and the registration extensions use it to register the data source, read the `DataSource` setting, and run the migration step. Endpoints do not use Infrastructure types.

The API never uses EF Core types directly, and the Application layer never references a framework that is specific to the database.

## Consequences

- Business rules live in one place (`ProductFactory`) and are used by both the EF Core and the mock repositories.
- The data source can be changed without touching the endpoints (see [0006](0006-selectable-data-source.md)).
- The composition root is the place that knows about Infrastructure, which is the standard arrangement for ASP.NET Core applications.
- `Product` is both the domain entity and the EF Core entity. A separate persistence model would add mapping code without a clear benefit for this CRUD scope. If the domain grows, this decision should be revisited.

## Alternatives considered

- **Single project with folders.** Simpler, but nothing enforces the dependency rules, and that is the main benefit of the split.
- **Separate persistence model.** More code to map between two models. Rejected for now because the domain is simple.
