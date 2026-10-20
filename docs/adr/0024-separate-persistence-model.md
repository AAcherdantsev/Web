# 0024. Separate persistence model

Date: 2026-10-20
Status: Accepted. Supersedes the decision in [0002](0002-layered-architecture.md) that `Product` is both the domain entity and the EF Core entity.

## Context

`Product` was the domain entity and the EF Core entity at the same time. Its row version was a property of the domain class, and the domain had to expose `Restore`, so that the repositories could rebuild a product from stored values. A persistence concern was therefore part of the domain model.

The mock store also kept `Product` objects, so the in-memory data source depended on the domain type as its storage shape.

## Decision

* `Product` in `Web.Domain` contains only domain state and rules. It has no row version.
* `ProductRecord` in `Web.Infrastructure` is the stored form. It has settable properties for EF Core, the `RowVersion` column, and the mapping to and from `Product`.
* `ProductRepository` and `MockProductRepository` both store `ProductRecord` objects. They map to `Product` on every read, so a caller never holds a stored object.
* The row version is not part of the domain. It travels as an opaque token next to the entity, in `VersionedProduct` (Application). The application service passes back the token it read, and the store checks it on update. The application layer never reads or creates the token's content.
* `Product.Restore` is still public. It is the domain's way to rebuild an entity from stored values, and it checks the same rules as a new product. It no longer takes a row version.
* The EF Core table and columns are unchanged. The migration `ProductRecordPersistenceModel` is empty, because only the model's entity name changed.

## Consequences

* The domain has no persistence concerns, and its tests need no database or EF Core types.
* The mapping between the record and the entity is code that must be kept in step with both. For six properties this is small, and the SQL and mock tests exercise it.
* The version token is a byte array in the application layer. It is opaque, and the only code that interprets it is the store. This is a deliberate compromise: a typed version object would also need to live in the application layer and would not be simpler.
* Updates are made on a detached record that carries the version that was read. If the context already tracks a record with the same key, the repository detaches it first, so the update does not fail for a reason the caller cannot see.

## Alternatives considered

* **Keep `Product` as the EF Core entity.** Fewer classes, but the domain keeps persistence concerns and the public `Restore`. Rejected, because the goal is that each layer has its own model.
* **Keep the row version on `Product` as a shadow property.** Hides the column from the domain, but the mock store and the tests would still need to know about it. Rejected, because the version belongs to the store, not to the entity.
* **Keep the row version on `Product` with a `RowVersion` property but no EF mapping in the domain.** This still puts the version in the domain class. Rejected for the same reason.
