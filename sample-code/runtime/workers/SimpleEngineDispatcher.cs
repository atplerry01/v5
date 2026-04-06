using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Whycespace.Runtime.Workers;

public sealed class SimpleEngineDispatcher : BackgroundService
{
    private readonly ILogger<SimpleEngineDispatcher> _logger;

    public SimpleEngineDispatcher(ILogger<SimpleEngineDispatcher> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SimpleEngineDispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("SimpleEngineDispatcher stopped");
    }
}
