# 0008. URL-based API versioning

Date: 2026-10-16
Status: Accepted

## Context

The task asks for a second version of the product list endpoint with pagination, and for the versioning method to be reflected in the API documentation. Version 1 must keep working for existing clients.

## Decision

The version is part of the URL path: `/v1/products` and `/v2/products`. FastEndpoints `Version(n)` assigns each endpoint to a version. The Swagger documentation has one document per version (`/swagger/v1/swagger.json` and `/swagger/v2/swagger.json`).

Only the list endpoint changes in v2. Other endpoints remain in v1 and are documented there.

## Consequences

- The version is visible in every URL, in logs, and in the browser, which makes debugging easier.
- Each version can be cached and linked independently.
- Swagger shows exactly which endpoints belong to each version.
- A new version of a resource duplicates its routes. This is acceptable for two versions.

## Alternatives considered

- **Header-based versioning** (`api-version` header). Keeps URLs clean, but versions are hidden from browsers and from simple links, and documentation is harder to read.
- **Query-string versioning.** Easy to add, but it mixes the version with filter parameters and is easy to forget.
- **Media-type versioning.** Standards-based, but the most complex option for clients to use.
