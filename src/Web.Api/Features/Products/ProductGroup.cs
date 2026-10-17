using FastEndpoints;

namespace Web.Api.Features.Products;

public sealed class ProductGroup : Group
{
    private const string ENDPOINT_GROUP_PRODUCTS = "products";

    public ProductGroup()
    {
        Configure($"/{ENDPOINT_GROUP_PRODUCTS}", ep =>
        {
            ep.AllowAnonymous();
        });
    }

}

