# 0016. Anonymous access for the assignment

Date: 2026-10-16
Status: Accepted

## Context

Every endpoint is anonymous, so anyone who can reach the API can create products and change product descriptions. Authentication (JWT bearer, roles or scopes) is the usual fix for this.

The project is a test assignment. The specification asks for REST endpoints, Swagger, and tests, and it does not ask for authentication. Adding an identity provider or a token service would add substantial scope, and it would not be tested in the assignment's environment.

## Decision

The endpoints stay anonymous. This is an accepted risk for this assignment. The risk is:

* anyone who can reach the API can create products and change product descriptions;
* rate limiting (ADR 0015) slows abuse but does not prevent it.

The service must not be exposed to untrusted networks in this form. Before any real deployment, add authentication to the write endpoints (`POST`, `PATCH`) at least, then decide what the read endpoints need.

## Consequences

* The assignment stays within its scope.
* The risk is recorded here and in the security checklist of `docs/operations.md`, so that it is not forgotten when the service moves beyond the assignment.
* Deployments follow the security checklist in `docs/operations.md`.
* The Docker image (see the `Dockerfile` and the README) runs the AppHost in the Development environment, so Swagger is enabled and the endpoints are anonymous. The image is for local runs and the assignment, and its ports must not be published to untrusted networks.

## Alternatives considered

* **Basic or API-key authentication now.** Quicker than JWT, but it still needs key management, and it would not match the identity model that a real shop would use.
* **Authentication through an API gateway.** The right place for it in a larger system. The gateway is not part of this repository.
