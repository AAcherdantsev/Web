using Web.Domain.Exceptions;

namespace Web.Domain.Entities;

/// <summary>
///     Domain entity representing a product. Its values change only through the methods of this class, and every
///     value is checked against <see cref="ProductRules" /> when it is set.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    ///     Constructor used by Entity Framework Core when it loads a product.
    /// </summary>
    private Product()
    {
    }

    /// <summary>
    ///     Creates a new product that has not been stored yet.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="imgUri">URI of the product image.</param>
    /// <param name="price">Price of the product.</param>
    /// <param name="description">Description of the product.</param>
    /// <param name="createdAt">Date and time when the product was created.</param>
    /// <exception cref="InvalidProductException">Thrown when a value breaks a business rule.</exception>
    public Product(string name, string imgUri, decimal price, string description, DateTimeOffset createdAt)
    {
        IReadOnlyList<string> violations = ProductRules.Violations(name, imgUri, price, description);
        if (violations.Count > 0)
        {
            throw new InvalidProductException(violations);
        }

        Name = name;
        ImgUri = imgUri;
        Price = price;
        Description = description;
        CreatedAt = createdAt;
        LastUpdatedAt = createdAt;
    }

    /// <summary>
    ///     Rebuilds a product from the values held by a store. Only the persistence adapters call this, because the
    ///     identifier is assigned by the store. The values are checked against the same rules as a new product, and the
    ///     application layer never calls this method.
    /// </summary>
    /// <exception cref="InvalidProductException">Thrown when a stored value breaks a business rule.</exception>
    public static Product Restore(int id, string name, string imgUri, decimal price, string description,
        DateTimeOffset createdAt, DateTimeOffset lastUpdatedAt)
    {
        return new(name, imgUri, price, description, createdAt)
        {
            Id = id,
            LastUpdatedAt = lastUpdatedAt
        };
    }

    /// <summary>
    ///     The product name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     URI of the product image.
    /// </summary>
    public string ImgUri { get; private set; } = string.Empty;

    /// <summary>
    ///     Price of the product.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    ///     Description of the product. Empty when the product has no description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    ///     Date and time when the product was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Date and time of the last change.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>
    ///     Replaces the description of the product and records the time of the change.
    /// </summary>
    /// <param name="description">The new description. Empty when the product has no description.</param>
    /// <param name="updatedAt">The time of the change.</param>
    /// <exception cref="InvalidProductException">Thrown when the description breaks a business rule.</exception>
    public void ChangeDescription(string description, DateTimeOffset updatedAt)
    {
        IReadOnlyList<string> violations = ProductRules.DescriptionViolations(description);
        if (violations.Count > 0)
        {
            throw new InvalidProductException(violations);
        }

        Description = description;
        LastUpdatedAt = updatedAt;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Id:{Id}, Name:{Name}, Price:{Price}, ImgUri:{ImgUri}, " +
               $"Description:{(string.IsNullOrEmpty(Description) ? "-" : Description)}, " +
               $"Created:{CreatedAt:dd.MM.yyyy HH:mm:ss}, LastUpdatedAt:{LastUpdatedAt:dd.MM.yyyy HH:mm:ss}";
    }
}
