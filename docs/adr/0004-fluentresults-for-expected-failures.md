# 0004. Use FluentResults for expected failures

Date: 2026-10-16
Status: Accepted

## Context

Some failures are expected and part of normal behaviour: a product does not exist, or the input breaks a business rule. Other failures are unexpected, such as a lost database connection. Using exceptions for both makes control flow hard to read, because every caller needs a `try/catch` or must know which exceptions a method can throw.

## Decision

The application services return `Result<T>` from **FluentResults 4.0** for expected outcomes. The repositories return the stored values, or `null` when a product is missing, and they throw on unexpected failures. A failure carries typed errors, such as `NotFoundError`, or plain messages, such as "Price cannot be negative.".

Rules:

1. Expected failures are returned as failed results (`NotFoundError`, `ValidationError`, `ConflictError`).
2. Unexpected failures are thrown as exceptions. Repositories only catch `DbUpdateConcurrencyException` to translate it into `ProductConflictException` (see [0021](0021-optimistic-concurrency-with-row-version.md)). The global exception handler turns unhandled exceptions into HTTP 500 responses (see [0009](0009-problem-details-for-errors.md)).
3. Endpoints map any failed result to an HTTP response with `ProblemResults.ForErrors(result.Errors)`.

## Consequences

- The contract is visible in the method signature: callers know that a product might be missing, and they handle it.
- Typed errors let endpoints choose the status code without parsing strings.
- Database and infrastructure errors reach the client as 500, not 400.
- We take on a dependency. FluentResults is MIT-licensed and small, but its implicit conversions (`return product;` for `Result<Product>`) can hide intent.
- Rule 2 has to be kept. A repository that catches every exception would turn outages into client errors. The integration test `GetProduct_WhenTheServiceThrows_Returns500ProblemDetails` in `ErrorHandlingTests` verifies the HTTP 500 behaviour.

The repository and endpoints use only `IsSuccess`, `Value`, `Errors`, and `HasError<T>()`, so the library could be replaced with a project-owned result type without changing the design.

## Alternatives considered

- **Exceptions for everything.** Simple to write, but callers cannot see the failure modes, and expected cases end up costing more than they should.
- **Project-owned `Result<T>` record.** Dependency-free and fully under control. Rejected for now because FluentResults already gives typed errors and metadata, and the team does not need to maintain that code.
- **OneOf or ErrorOr.** Similar goals. We picked FluentResults because it provides typed errors and metadata with a small API.
