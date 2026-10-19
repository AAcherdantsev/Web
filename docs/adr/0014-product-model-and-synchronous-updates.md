# 0014. Product model as specified and synchronous updates

Date: 2026-10-16
Status: Accepted

## Context

The task defines a product with `Id`, `Name`, `ImgUri`, `Price`, and an optional `Description`. It asks for an endpoint that updates the product description. It does not mention stock quantity or asynchronous processing.

## Decision

The `Product` entity has only the attributes from the specification. The description update is a synchronous `PATCH /v1/products/{id}`: the request is processed in the HTTP call, and the response shows the result.

## Consequences

- The system is simple to run, operate, and test. It needs only the API and, in the database mode, SQL Server.
- Stock levels cannot be tracked. If inventory is needed later, it should be designed as its own capability with its own record.
- Asynchronous processing is not available. If it is needed, it should be introduced with an ADR of its own.

## Alternatives considered

- **Asynchronous updates through a message broker.** The client would receive `202 Accepted` and check the result later. This adds a broker, a consumer, and infrastructure to run and test, and the specification does not ask for it. A synchronous `200` is also simpler for clients to use correctly.
- **Stock quantity as an extra field.** Not in the specification. Every extra field increases the surface that has to be tested, validated, and secured.
