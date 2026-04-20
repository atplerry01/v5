using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.4 — hosted-service shell driving
/// <see cref="OutboundEffectReconciliationSweeper.SweepOnceAsync"/> in a
/// loop. Poll cadence defaults to the relay's configured interval —
/// operators needing tighter finality-latency tune it via
/// <c>OutboundEffects:Relay:PollIntervalMs</c>.
/// </summary>
public sealed class OutboundEffectReconciliationSweeperWorker : BackgroundService
{
    private readonly OutboundEffectReconciliationSweeper _sweeper;
    private readonly OutboundEffectRelayOptions _options;
    private readonly ILogger<OutboundEffectReconciliationSweeperWorker> _logger;

    public OutboundEffectReconciliationSweeperWorker(
        OutboundEffectReconciliationSweeper sweeper,
        OutboundEffectRelayOptions options,
        ILogger<OutboundEffectReconciliationSweeperWorker> logger)
    {
        _sweeper = sweeper;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboundEffectReconciliationSweeperWorker starting (hostId={HostId}, batch={Batch}, pollMs={Poll})",
            _options.HostId, _options.BatchSize, _options.PollIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await _sweeper.SweepOnceAsync(stoppingToken);
                if (processed == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(_options.PollIntervalMs), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboundEffectReconciliationSweeperWorker poll failed; sleeping before retry.");
                try { await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
