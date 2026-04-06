using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicOptimization;

/// <summary>
/// Pure delegation handler. No orchestration, no chaining.
/// </summary>
public sealed class OptimizationCommandHandler
{
    private readonly EconomicOptimizationEngine _engine;

    public OptimizationCommandHandler(IEconomicIntelligenceDomainService domainService)
    {
        _engine = new EconomicOptimizationEngine(domainService);
    }

    public Task<EngineResult> HandleAsync(
        GenerateEconomicOptimizationCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default) => command switch
    {
        GenerateEconomicOptimizationCommand cmd => _engine.ExecuteAsync(cmd, context, cancellationToken),
        _ => Task.FromResult(EngineResult.Fail(
            $"Unknown command: {command.GetType().Name}", "UNKNOWN_COMMAND"))
    };
}
