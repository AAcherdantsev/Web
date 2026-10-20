# 0021. Optimistic concurrency with a row version

Date: 2026-10-20
Status: Accepted

## Context

`PATCH /v1/products/{id}` reads a product, changes its description, and saves it. Two requests that run at the same time can both read the same product. Without a check, the second save overwrites the first, and neither request learns about it. This is a lost update.

The assignment does not ask for locking, and the write is short. A reviewer will still ask what happens in a race, so the answer must be in the code and in the documentation.

## Decision

* Each product row has a `RowVersion`, a SQL Server `rowversion` column that the database changes on every update. The persistence record `ProductRecord` holds it, and EF Core maps it with `IsRowVersion()`. The domain entity has no version (see [0024](0024-separate-persistence-model.md)).
* The store returns the version with the product in a `VersionedProduct`. The application service passes it back unchanged when it saves the product.
* An update saves the product with the version that was read. If another request saved the product in between, no row matches the `UPDATE`. EF Core raises `DbUpdateConcurrencyException`, and the repository throws `ProductConflictException`.
* The service turns that exception into a `ConflictError`, and the endpoint returns `409 Conflict` as RFC 7807 problem details. The client reads the product again and sends the update again.
* The `Mock` store uses its own version counter and makes the same check. It throws the same exception for a stale copy and for a product that does not exist, so both data sources behave the same way.
* The server does not retry by itself. A retry would hide the race from the client.

## Consequences

* A lost update cannot happen. The request that loses the race gets `409` instead of overwriting the other change.
* Clients must handle `409`. The description is a complete replacement, so sending the same request again is safe.
* The migration `AddProductRowVersion` adds the column. The existing rows get a version automatically, and no data is changed.
* Clients cannot yet tell which version they read, because the API does not send an `ETag` and does not accept `If-Match`. A client therefore cannot avoid the `409` by checking first. This is the next step if clients need read-modify-write with a guaranteed version. It is not implemented now.

## Alternatives considered

* **Last write wins, without a check.** The simplest option, but it loses updates silently.
* **Pessimistic locking** (a lock held from the read to the write). It keeps a database lock open across the request and is not needed for writes this short.
* **Retry on the server.** A description update could safely be repeated, but the retry would hide the race. The explicit `409` makes the conflict visible to the client.
* **`ETag` and `If-Match` now.** This gives a stronger guarantee: a client can detect a change before it sends its update. It requires clients to read the `ETag` first, and the assignment does not ask for it.
