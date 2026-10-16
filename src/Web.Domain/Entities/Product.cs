namespace Web.Domain.Entities;

/// <summary>
///     Domain entity representing a product.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    ///     The product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     URI of the product image.
    /// </summary>
    public required string ImgUri { get; set; }

    /// <summary>
    ///     Price of the product.
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    ///     Description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Date and time when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    ///     Date and time when the product was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Id:{Id}, Name:{Name}, Price:{Price}, ImgUri:{ImgUri}, " +
               $"Description:{(string.IsNullOrEmpty(Description) ? "-" : Description)}, " +
               $"Created:{CreatedAt:dd.MM.yyyy hh:mm:ss}, LastUpdatedAt:{LastUpdatedAt:dd.MM.yyyy hh:mm:ss}";
    }
}
