# 0012. Validation in two layers

Date: 2026-10-16
Status: Partially superseded by [0022](0022-domain-entity-enforces-invariants.md)

## Context

Input can be wrong in two ways: it can have the wrong shape (a missing field, a value out of range, a malformed URI), or it can break a business rule (a negative price). Both must be rejected, but they belong to different layers.

## Decision

- **HTTP layer.** FastEndpoints validators (`Validator<T>`) check the request shape: required values, structural validation, and type correctness. They run before the endpoint, and a failure returns a 400 Bad Request with a standardized Problem Details payload that includes the specific validation error codes and reasons (see [0009](0009-problem-details-for-errors.md)).
- **Domain layer.** The business rules (for example: name not empty, price greater than 0 with at most two decimal places, an `http` or `https` image address, and length limits) are defined once, as predicates in `ProductRules`. The entity checks them when a product is created or changed, and the Application layer exposes them through `ProductValidation`.

Domain entities (`Product`) enforce the business rules themselves, so no entry point can create or change a product with invalid values. The `ProductValidationRules` extensions remain for the HTTP request validators, which report field-level errors before any work is done. See [0022](0022-domain-entity-enforces-invariants.md).

## Consequences

- Clients get complete, field-level errors with specific error codes before any work is done.
- **Reduced duplication:** Because business rules are centralized in `ProductValidationRules` as extension methods, they no longer need to be written twice (once for the HTTP shape and once for the Domain). When a rule changes, it only needs to be updated in one place.
- Business rules cannot be bypassed by a new endpoint, provided the new models apply the shared application-level validation rules.
- Malformed JSON and type mismatches are rejected by the framework before validation, with a 400 response.

## Alternatives considered

- **Validation only in the controller or endpoint.** Simple, but the rules are lost for any other entry point.
- **Validation only in the domain (e.g., static ProductRules).** Clients receive only the first error, and the domain needs to be manually mapped to HTTP error shapes. This also leads to duplicating some structural checks.
- **DataAnnotations.** Works with the framework, but it cannot express the cross-field and configuration-based rules that the pagination limit needs.
