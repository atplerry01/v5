using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Orchestration;

/// <summary>
/// Delegates demand forecasting to the T3I forecasting engine via intent dispatch.
/// No local calculations — all business logic lives in engines.
/// </summary>
public sealed class DemandForecastService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public DemandForecastService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> ForecastAsync(
        string clusterId,
        string serviceType,
        int horizonDays,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"DemandForecastService:ForecastAsync:{clusterId}:{serviceType}:{horizonDays}"),
            CommandType = "atlas.forecast.demand",
            Payload = new { ClusterId = clusterId, ServiceType = serviceType, HorizonDays = horizonDays },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:DemandForecastService:{clusterId}:{serviceType}:{horizonDays}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
