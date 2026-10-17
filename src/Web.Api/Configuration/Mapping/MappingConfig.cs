using Mapster;
using Web.Api.Models;
using Web.Application.Common;
using Web.Domain.Entities;

namespace Web.Api.Configuration.Mapping;

/// <summary>
///     Defines the Mapster mappings from domain entities to API response models.
/// </summary>
/// <remarks>
///     <see cref="Product" /> to <see cref="ProductDto" /> is mapped by matching property names,
///     so only the paged list needs an explicit mapping.
/// </remarks>
public sealed class MappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PagedList<Product>, PagedList<ProductDto>>()
            .MapWith(src => new(
                src.Values.Adapt<List<ProductDto>>(),
                src.TotalCount,
                src.PageNumber,
                src.PageSize));
    }
}
