# 0012. Validation in two layers

Date: 2026-10-16
Status: Accepted

## Context

Input can be wrong in two ways: it can have the wrong shape (a missing field, a value out of range, a malformed URI), or it can break a business rule (a negative price). Both must be rejected, but they belong to different layers.

## Decision

- **HTTP layer.** FluentValidation validators (`Validator<T>` in FastEndpoints) check the request shape: required values, ranges, URI syntax, and the configured page size limit. They run before the endpoint, and a failure returns 400 with the errors listed (see [0009](0009-problem-details-for-errors.md)).
- **Application layer.** `ProductRules` and `ProductFactory` check the business rules when a product is created: name not blank, price greater than 0 with at most two decimal places, an `http` or `https` image address, and the length limits. `ProductService` checks the description rules when a description changes. The use cases run for every data source, so no rule depends on the endpoint or the repository.

Some checks exist in both layers. This is intentional: the HTTP validators give clients precise, field-level messages, and the factory protects the rules for any future caller that does not go through HTTP.

## Consequences

- Clients get complete, field-level errors before any work is done.
- Business rules cannot be bypassed by a new endpoint or a background job.
- The same rule is written twice (for example the URI check). When a rule changes, both places must change. The unit tests in both projects make a missed change visible.
- Malformed JSON and type mismatches are rejected by the framework before validation, with a 400 response.

## Alternatives considered

- **Validation only in the controller or endpoint.** Simple, but the rules are lost for any other entry point.
- **Validation only in the domain.** Clients receive only the first error, and the domain would need to know about HTTP error shapes.
- **DataAnnotations.** Works with the framework, but it cannot express the cross-field and configuration-based rules that the pagination limit needs.
