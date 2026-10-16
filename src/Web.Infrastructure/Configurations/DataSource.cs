namespace Web.Infrastructure.Configurations;

/// <summary>
///     Selects where product data is stored. Configured with the <c>DataSource</c> setting.
/// </summary>
public enum DataSource
{
    /// <summary>
    ///     Product data is stored in SQL Server through Entity Framework Core.
    /// </summary>
    Database,

    /// <summary>
    ///     Product data is kept in memory and pre-filled with seed data. No database is required.
    /// </summary>
    Mock
}
