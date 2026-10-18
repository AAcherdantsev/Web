# 0019. Input limits and stable paging order

Date: 2026-10-16
Status: Accepted

## Context

Three requirements shape the product data and the product list:

* `Name`, `Description`, and `ImgUri` must have bounded sizes. Without limits a client could store very large values, and the request body would be limited only by the server default.
* The product list must be paged without gaps or duplicates. Products can share the same `CreatedAt` value, for example when they are created in one batch, so ordering by `CreatedAt` alone does not define their order.
* Paging must not sort the whole table on every request.

## Decision

* **Limits** are defined once, in `ProductLimits` in the Domain project: name 200 characters, image URI 2048, description 4000. They are used by:
  * the request validators, which return a 400 with a field-level message;
  * the business rules in `ProductFactory`, which protect callers that do not use HTTP;
  * the database schema, through `HasMaxLength` in the `InitialCreate` migration, as a last line of defence.
* **Request body** size is limited to 64 KB (`Kestrel:Limits:MaxRequestBodySize`).
* **Ordering** is `CreatedAt`, then `Id`. The second key makes the order total, so paging is stable.
* **Index** on `(CreatedAt, Id)` supports that ordering.

The limits are generous for product data. Changing them requires a change in `ProductLimits`, the validators, and a new migration.

## Consequences

* Oversized input is rejected with a clear message before any work is done.
* Paging is consistent, and an integration test against SQL Server checks that no product appears on two pages.
* The index adds a small cost on each insert.
* Changing a limit in the database requires a new migration. A row that already exceeds a new limit makes that migration fail, so the migration must be tested against a copy of the data first.

## Alternatives considered

* **Keyset (cursor) paging.** Would make paging faster for deep pages, and it needs a stable order too. Offset paging is kept (ADR 0010), and it is adequate at this scale.
* **Limits only in the validators.** Simpler, but a second entry point (for example a future background job) would bypass them.
