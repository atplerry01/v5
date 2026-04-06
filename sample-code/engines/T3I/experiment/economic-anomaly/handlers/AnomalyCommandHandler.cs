using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicAnomaly;

/// <summary>
/// Pure delegation handler. No orchestration, no chaining.
/// </summary>
public sealed class AnomalyCommandHandler
{
    private readonly EconomicAnomalyEngine _engine;

    public AnomalyCommandHandler(IEconomicIntelligenceDomainService domainService)
    {
        _engine = new EconomicAnomalyEngine(domainService);
    }

    public Task<EngineResult> HandleAsync(
        DetectEconomicAnomalyCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default) => command switch
    {
        DetectEconomicAnomalyCommand cmd => _engine.ExecuteAsync(cmd, context, cancellationToken),
        _ => Task.FromResult(EngineResult.Fail(
            $"Unknown command: {command.GetType().Name}", "UNKNOWN_COMMAND"))
    };
}
