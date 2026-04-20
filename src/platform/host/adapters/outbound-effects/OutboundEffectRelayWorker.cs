using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.1 — hosted-service shell around <see cref="OutboundEffectRelay"/>.
/// Matches the existing adapter-layer pattern (workers in
/// <c>src/platform/host/adapters</c> wrap runtime-layer pollers). No
/// per-message fairness or leader-election in R3.B.1 — the relay uses the
/// <c>claimed_by</c> column for multi-host safety (R-OUT-EFF-OUTBOX-03).
/// </summary>
public sealed class OutboundEffectRelayWorker : BackgroundService
{
    private readonly OutboundEffectRelay _relay;
    private readonly OutboundEffectRelayOptions _options;
    private readonly ILogger<OutboundEffectRelayWorker> _logger;

    public OutboundEffectRelayWorker(
        OutboundEffectRelay relay,
        OutboundEffectRelayOptions options,
        ILogger<OutboundEffectRelayWorker> logger)
    {
        _relay = relay;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboundEffectRelayWorker starting (hostId={HostId}, batch={Batch}, pollMs={Poll})",
            _options.HostId, _options.BatchSize, _options.PollIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await _relay.PollOnceAsync(stoppingToken);
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
                _logger.LogError(ex, "OutboundEffectRelayWorker poll failed; sleeping before retry.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        _logger.LogInformation("OutboundEffectRelayWorker stopped.");
    }
}
