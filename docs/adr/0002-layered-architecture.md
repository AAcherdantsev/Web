# 0002. Layered architecture with a single dependency direction

Date: 2026-10-16
Status: Accepted

## Context

The task requires a layered architecture and SOLID design. The HTTP framework, the database, and the business rules change for different reasons, so they should not be tangled together.

## Decision

The solution has four source projects with these dependencies:

- `Web.Domain` depends on nothing. It contains the `Product` entity, the business rules of a product (`ProductRules`), and the field limits (`ProductLimits`).
- `Web.Application` depends on Domain. It contains the use cases (`IProductService` and `ProductService`), the commands and queries, the application models (`ProductModel`), the error types, the repository contract (`IProductRepository`), and the facade `ProductValidation`, which publishes the domain rules for the HTTP layer (see [0022](0022-domain-entity-enforces-invariants.md)). The use cases get the current time from `TimeProvider`, so tests can control it.
- The endpoints call the use cases only. They do not use the repository.
- `Web.Infrastructure` depends on Application and Domain. It implements the repository with EF Core, provides the in-memory mock, and registers services.
- `Web.Api` depends on Application only; it does not reference Domain, so domain types do not reach the HTTP layer (see [0022](0022-domain-entity-enforces-invariants.md)). The request validation rules are in `Web.Api/Validation`, and they read the domain rules through `ProductValidation`.
- `Web.Api` also references Infrastructure, but only for the composition root: `Program.cs` and the registration extensions use it to register the data source, read the `DataSource` setting, and run the migration step. This is an accepted exception. Endpoints do not use Infrastructure types, and the exception is limited to the composition root.

The API never uses EF Core types directly, and the Application layer never references a framework that is specific to the database.

## Consequences

- Business rules live in the Domain layer. Both the EF Core and the mock repositories only store products, so a rule cannot differ between the data sources.
- The data source can be changed without touching the endpoints (see [0006](0006-selectable-data-source.md)).
- The composition root is the place that knows about Infrastructure, which is the standard arrangement for ASP.NET Core applications. If the exception becomes a problem, the registration code can move to its own project.
- `Product` is the domain entity. The EF Core entity is a separate persistence model, `ProductRecord`, which was introduced later (see [0024](0024-separate-persistence-model.md)). This decision was revisited when the row version was found to be a persistence concern.

## Alternatives considered

- **Single project with folders.** Simpler, but nothing enforces the dependency rules, and that is the main benefit of the split.
- **Separate persistence model.** More code to map between two models. Rejected at first because the domain was simple. Adopted later, see [0024](0024-separate-persistence-model.md).
