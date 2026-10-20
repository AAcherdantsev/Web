using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Web.Infrastructure.Configurations;

namespace Web.Infrastructure.Services;

/// <summary>
///     Hosted service that applies database migrations and seeds example products during application startup,
///     as configured by <see cref="DatabaseOptions" />.
/// </summary>
public sealed class StartupInitializer : IHostedService
{
    private readonly IOptions<DatabaseOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new instance of the <see cref="StartupInitializer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The database startup settings.</param>
    public StartupInitializer(IServiceProvider serviceProvider, IOptions<DatabaseOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <summary>
    ///     Starts the hosted service. Applies migrations and seeds example products when the settings allow it.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the initialization process.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Value.MigrateOnStartup)
        {
            return;
        }

        await DatabaseInitializer.InitializeAsync(_serviceProvider, _options.Value.SeedSampleData);
    }

    /// <summary>
    ///     Stops the hosted service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
