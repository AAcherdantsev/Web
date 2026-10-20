# 0022. The domain entity enforces its invariants

Date: 2026-10-20
Status: Accepted. Partially supersedes [0012](0012-validation-in-two-layers.md).

## Context

The rules of a product (name, price, image address, and length limits) were written as FluentValidation validators in the Application layer. `ProductFactory` and `ProductService` created those validators with `new`. That hides a dependency, and a test cannot replace it.

The `Product` entity accepted any value. Any code that did not go through the validators could create a product with an empty name or a negative price. The entity also exposed `AssignId` and a public `Restore` method, so any code could change an identifier, and the mock store used `Restore` to copy products.

## Decision

* **The entity checks its rules.** The `Product` constructor and `ChangeDescription` check the values with `ProductRules` in the Domain project. A broken rule throws `InvalidProductException`, which lists every broken rule. The messages are defined once, in `ProductRules`.
* **The services check the same rules before they change anything.** `ProductService` returns the broken rules as `ValidationError` results. The expected failure then follows the result pattern of [0004](0004-fluentresults-for-expected-failures.md), and no exception is thrown in the normal flow.
* **The HTTP validators stay, in `Web.Api/Validation`.** The FluentValidation rules reject bad input early, with field-level messages and error codes. They call the predicates through the facade `ProductValidation` in the Application layer, so they add no rule of their own, and `Web.Api` does not reference the domain project.
* **The Application layer has no validators of its own for the commands.** `ProductFactory`, `CreateProductCommandValidator`, and `UpdateProductDescriptionValidator` are removed.
* **Domain types stay inside the application.** `IProductService` returns `ProductModel`, a DTO of the Application layer. `Web.Api` no longer references `Web.Domain`.
* **Rebuilding an entity from storage is public, and only persistence uses it.** `Product.Restore` is public, and its documentation says that only the persistence adapters call it. It checks the same rules as a new product. The `InternalsVisibleTo` friend assembly is removed, so the domain does not know about Web.Infrastructure. `AssignId` is removed. The stores return the saved product from `AddAsync` and `UpdateAsync`, so the identifier and row version come from the store and the caller's object is not changed.

## Consequences

* A `Product` cannot be created or changed into an invalid state, whatever the caller is.
* Each business rule is defined once, in `ProductRules`, as a predicate and a message. The entity and the HTTP validators both call the same predicate, so the rule cannot drift between the two layers. The HTTP validators still report field-level errors and error codes before any work is done.
* The Api project depends on the Application layer only. Changes to the entity do not reach the HTTP models.
* The rules are tested in `Web.Domain.Tests`, which tests the predicates directly. The validators in `Web.Api` are tested for the field-level messages and codes.
* Store implementations return the saved product instead of changing the object they were given. The mock store copies products on every read and write, as before.

## Alternatives considered

* **Keep the rules in the Application validators only.** Simple, but the entity accepts any value, and each caller must remember to validate.
* **A factory that returns a result from the Domain.** This avoids exceptions, but the entity would depend on a result type. Exceptions in the entity and results in the service keep each layer's responsibility clear.
