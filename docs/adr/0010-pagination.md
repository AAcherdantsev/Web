# 0010. Pagination with configurable default and maximum page size

Date: 2026-10-16
Status: Accepted

## Context

Version 2 of the product list must paginate, with a default page size of 10. An unbounded page size lets a single request read the whole table, so the page size needs an upper limit. The default and the limit should be changeable without recompiling.

## Decision

- `pageNumber` is zero-based and defaults to `0`. Negative values are rejected.
- `pageSize` defaults to `Pagination:DefaultPageSize` (`10`). Values outside `1..Pagination:MaxPageSize` (`100`) are rejected with 400.
- The offset of a page (`pageNumber × pageSize`) must fit in an `int`, because the database query uses an `int` offset. A larger combination is rejected with 400 instead of returning a wrong page.
- `PaginationOptions` is bound from the `Pagination` configuration section. Validation runs at startup (`ValidateDataAnnotations` and `ValidateOnStart`), so a bad value stops the application before it serves requests. `DefaultPageSize` must not exceed `MaxPageSize`.
- To get all products, clients use `GET /v1/products`, which is a collection without pagination.
- The response is a `PagedList` with `pageNumber`, `pageSize`, `totalCount`, and `values`.

## Consequences

- The default and the maximum can be changed per environment with configuration.
- Clients cannot accidentally or deliberately read the whole table through v2.
- Changing the default changes the response for clients that omit `pageSize`. This is intended, but it should be communicated when it changes.
- Offset pagination (`pageNumber × pageSize`) becomes slower for very deep pages. For a catalogue of this size it is acceptable.

## Alternatives considered

- **Keyset (cursor) pagination.** More efficient for large tables and stable under inserts. More complex for clients, and it does not support jumping to an arbitrary page. Not needed at this scale.
- **Hard-coded defaults.** Simpler, but every change needs a rebuild.
