# Architecture Decision Records

This folder records the significant architecture decisions for the Web API solution. Each record explains the context, the decision, and its consequences, including the trade-offs we accepted.

Records are numbered and never deleted. When a decision changes, add a new record that supersedes the old one and update the old record's status.

| No. | Title | Status |
| --- | --- | --- |
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-layered-architecture.md) | Layered architecture with a single dependency direction | Accepted |
| [0003](0003-fastendpoints-for-http-layer.md) | Use FastEndpoints for the HTTP layer | Accepted |
| [0004](0004-fluentresults-for-expected-failures.md) | Use FluentResults for expected failures | Accepted |
| [0005](0005-sql-server-and-entity-framework-core.md) | Store data in SQL Server with Entity Framework Core | Accepted |
| [0006](0006-selectable-data-source.md) | Selectable data source (Database or Mock) | Accepted |
| [0007](0007-dotnet-aspire-for-local-hosting.md) | Use .NET Aspire for local orchestration | Accepted |
| [0008](0008-url-based-api-versioning.md) | URL-based API versioning | Accepted |
| [0009](0009-problem-details-for-errors.md) | RFC 7807 problem details for error responses | Accepted |
| [0010](0010-pagination.md) | Pagination with configurable default and maximum page size | Accepted |
| [0011](0011-mapster-for-object-mapping.md) | Use Mapster for object mapping | Accepted |
| [0012](0012-validation-in-two-layers.md) | Validation in two layers | Accepted |
| [0013](0013-central-package-management.md) | Central package management | Accepted |
| [0014](0014-product-model-and-synchronous-updates.md) | Product model as specified and synchronous updates | Accepted |
| [0015](0015-rate-limiting.md) | Rate limiting with token buckets per client | Accepted |
| [0016](0016-anonymous-access-for-the-assignment.md) | Anonymous access for the assignment (accepted risk) | Accepted |
| [0017](0017-operational-endpoints-and-telemetry.md) | Operational endpoints and telemetry | Accepted |
| [0018](0018-migrations-as-a-separate-step.md) | Migrations as a separate step in Production | Accepted |
| [0019](0019-input-limits-and-paging-order.md) | Input limits and stable paging order | Accepted |
| [0020](0020-semantic-versioning-with-gitversion.md) | Semantic versions calculated from git history with GitVersion | Accepted |

## Template

```markdown
# NNNN. Title

Date: YYYY-MM-DD
Status: Proposed | Accepted | Superseded by NNNN

## Context

The forces that make this decision necessary.

## Decision

What we decided, stated in one or two sentences.

## Consequences

What becomes easier, what becomes harder, and what we accepted as trade-offs.

## Alternatives considered

The options we rejected and why.
```
