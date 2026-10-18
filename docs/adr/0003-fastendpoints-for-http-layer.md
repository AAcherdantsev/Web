# 0003. Use FastEndpoints for the HTTP layer

Date: 2026-10-16
Status: Accepted

## Context

The API needs versioned routes, request binding, validation, Swagger documentation per version, and consistent error responses. ASP.NET Core offers these through MVC controllers, Minimal APIs, or third-party libraries.

The API has five endpoints. A controller per resource would work for them as well.

## Decision

We use FastEndpoints 8.3 (with `FastEndpoints.Swagger` 8.3). Each endpoint is one class (`Endpoint<TRequest, TResponse>`), and it declares its route, version, Swagger summary, and validator in `Configure()`.

Reasons:

- **One class per endpoint.** Each endpoint has a single responsibility, and adding an endpoint does not change existing classes. This follows the single-responsibility and open/closed principles.
- **Built-in validation pipeline.** `Validator<T>` runs after binding and returns a 400 response automatically.
- **Versioning and Swagger.** `Version(n)` plus `FastEndpoints.Swagger` produces one Swagger document per version.
- **Error format.** `c.Errors.UseProblemDetails()` makes validation errors follow RFC 7807 (see [0009](0009-problem-details-for-errors.md)).

## Consequences

Accepted trade-offs:

- **Third-party dependency.** FastEndpoints is MIT-licensed and actively maintained, but the team has to learn its conventions (`Send.ResponseAsync`, `Send.CreatedAtAsync`, `Validator<T>()`).
- **Less familiar than MVC.** Most ASP.NET Core developers know controllers. New contributors need a short introduction.
- **Major-version upgrades can change APIs.** We pin the version centrally (see [0013](0013-central-package-management.md)).
- **Fewer examples online than for MVC.**

If the team prefers controllers, migration is contained: endpoints are thin and only call the repository, so each class can become an action method without changing the business logic.

## Alternatives considered

- **MVC controllers.** The default and the most familiar option. A reasonable choice. The deciding factor was the per-endpoint structure and the built-in validation pipeline.
- **Minimal APIs.** Lightweight, but they put all endpoints into one composition method, and they have no built-in validation pipeline or endpoint-level versioning helpers.
