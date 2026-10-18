# 0009. RFC 7807 problem details for error responses

Date: 2026-10-16
Status: Accepted

## Context

Clients need a predictable way to read errors. Validation, expected failures, and unexpected exceptions can each produce an error, and each must reach the client in the same format without exposing internal details.

## Decision

All error responses use **RFC 7807 problem details** with the `application/problem+json` content type.

- Validation errors come from FastEndpoints with `c.Errors.UseProblemDetails()`. Each failed field is listed in `errors` as `{ "name": ..., "reason": ... }`.
- Expected failures such as "not found" are built with `ProblemResults` in `Api/Errors`.
- Unexpected exceptions are handled by the global exception handler. It returns `500` with a generic title. The exception message and stack trace are shown only in the Development environment.
- Status codes follow the HTTP semantics: 400 for invalid input, 404 for a missing resource, 500 for unexpected failures.

## Consequences

- Clients parse one format for all errors.
- Internal details do not leak in production.
- The `errors` array uses FastEndpoints' field naming, which is camel-case property names. That is a convention rather than part of RFC 7807, and clients should read `errors[].name`.
- Problem responses do not have stable `type` URIs yet. Adding documentation for each error type would be the next step.

## Alternatives considered

- **Custom error envelope** (`{ "statusCode", "message", "errors" }`). Works, but it is not standard, so clients cannot reuse existing tooling for it.
- **Exceptions mapped to status codes globally.** Hides which expected failures a handler can return.
