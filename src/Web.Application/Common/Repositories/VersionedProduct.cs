using Web.Domain.Entities;

namespace Web.Application.Common.Repositories;

/// <summary>
///     A product together with the version it was read at. The version is opaque to the application: the store creates
///     it, and the store checks it when the product is saved. It is kept next to the entity, not inside it, so the
///     domain does not know about persistence.
/// </summary>
/// <param name="Product">The product.</param>
/// <param name="Version">The version the store had when it returned the product.</param>
public sealed record VersionedProduct(Product Product, byte[] Version);
