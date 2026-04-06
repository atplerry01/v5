using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Workers;

public class WorkflowWorker : BackgroundService
{
    private readonly ILogger<WorkflowWorker> _logger;
    private readonly IProjectionStore _projectionStore;
    private readonly IClock _clock;

    public WorkflowWorker(ILogger<WorkflowWorker> logger, IProjectionStore projectionStore, IClock clock)
    {
        _logger = logger;
        _projectionStore = projectionStore;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WorkflowWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _projectionStore.SetAsync(
                    "workflow", "heartbeat", _clock.UtcNowOffset.ToString("o"), stoppingToken);
                _logger.LogDebug("WorkflowWorker heartbeat written");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WorkflowWorker error");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
