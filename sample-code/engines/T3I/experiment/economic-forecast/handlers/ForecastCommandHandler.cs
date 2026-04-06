using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicForecast;

/// <summary>
/// Pure delegation handler. No orchestration, no chaining.
/// </summary>
public sealed class ForecastCommandHandler
{
    private readonly EconomicForecastEngine _engine;

    public ForecastCommandHandler(IEconomicIntelligenceDomainService domainService)
    {
        _engine = new EconomicForecastEngine(domainService);
    }

    public Task<EngineResult> HandleAsync(
        GenerateEconomicForecastCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default) => command switch
    {
        GenerateEconomicForecastCommand cmd => _engine.ExecuteAsync(cmd, context, cancellationToken),
        _ => Task.FromResult(EngineResult.Fail(
            $"Unknown command: {command.GetType().Name}", "UNKNOWN_COMMAND"))
    };
}
