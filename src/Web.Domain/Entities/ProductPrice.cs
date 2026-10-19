namespace Web.Domain.Entities;

/// <summary>
///     Rules for the price of a product. The database stores the price as <c>decimal(18,2)</c>.
/// </summary>
public static class ProductPrice
{
    /// <summary>
    ///     The largest price that <c>decimal(18,2)</c> can store.
    /// </summary>
    public const decimal MAX = 9_999_999_999_999_999.99m;

    /// <summary>
    ///     Checks that the price has at most two decimal places, so that no digits are lost when it is stored.
    /// </summary>
    /// <param name="price">The price to check.</param>
    public static bool HasAtMostTwoDecimalPlaces(decimal price)
    {
        return decimal.Round(price, 2) == price;
    }
}
