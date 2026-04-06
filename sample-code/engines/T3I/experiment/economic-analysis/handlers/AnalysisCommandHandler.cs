using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.EconomicAnalysis;

/// <summary>
/// Pure delegation handler. No orchestration, no chaining.
/// </summary>
public sealed class AnalysisCommandHandler
{
    private readonly EconomicAnalysisEngine _engine;

    public AnalysisCommandHandler(IEconomicIntelligenceDomainService domainService)
    {
        _engine = new EconomicAnalysisEngine(domainService);
    }

    public Task<EngineResult> HandleAsync(
        AnalyzeEconomicCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default) => command switch
    {
        AnalyzeEconomicCommand cmd => _engine.ExecuteAsync(cmd, context, cancellationToken),
        _ => Task.FromResult(EngineResult.Fail(
            $"Unknown command: {command.GetType().Name}", "UNKNOWN_COMMAND"))
    };
}
