using FastEndpoints;

namespace Web.Api.Features.Products;

/// <summary>
/// Represents a group of endpoints related to product management.
/// This group serves as a logical categorization for endpoints that perform
/// operations such as retrieving, creating, and updating product information.
/// </summary>
public sealed class ProductGroup : Group
{
    private const string ENDPOINT_GROUP_PRODUCTS = "products";

    public ProductGroup()
    {
        Configure($"/{ENDPOINT_GROUP_PRODUCTS}", ep => { ep.AllowAnonymous(); });
    }
}
