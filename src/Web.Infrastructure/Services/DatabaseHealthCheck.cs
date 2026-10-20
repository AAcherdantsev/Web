using Microsoft.Extensions.Diagnostics.HealthChecks;
using Web.Infrastructure.Persistence;

namespace Web.Infrastructure.Services;

/// <summary>
///     Reports whether the database accepts connections. Used by the readiness probe.
/// </summary>
internal sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly DatabaseContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseHealthCheck" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DatabaseHealthCheck(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("The database accepts connections.")
                : HealthCheckResult.Unhealthy("The database does not accept connections.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("The database check failed.", ex);
        }
    }
}
