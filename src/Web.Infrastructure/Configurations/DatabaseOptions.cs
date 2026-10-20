namespace Web.Infrastructure.Configurations;

/// <summary>
///     Database startup settings. Bound from the <c>Database</c> configuration section.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SECTION_NAME = "Database";

    /// <summary>
    ///     Whether the application applies pending migrations when it starts.
    ///     Production deployments should set this to <c>false</c> and run the <c>--migrate-only</c> step instead.
    /// </summary>
    public bool MigrateOnStartup { get; set; } = true;

    /// <summary>
    ///     Whether sample products are added when the table is empty.
    ///     Production deployments should set this to <c>false</c>.
    /// </summary>
    public bool SeedSampleData { get; set; } = true;
}
