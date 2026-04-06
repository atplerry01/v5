using Whycespace.Shared.Contracts.Domain.Intelligence.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.IntelligenceIntegrity;

/// <summary>
/// Pure delegation handler. No orchestration, no chaining.
/// </summary>
public sealed class IntegrityCommandHandler
{
    private readonly IntelligenceIntegrityEngine _engine;

    public IntegrityCommandHandler(IEconomicIntelligenceDomainService domainService)
    {
        _engine = new IntelligenceIntegrityEngine(domainService);
    }

    public Task<EngineResult> HandleAsync(
        EvaluateIntelligenceIntegrityCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default) => command switch
    {
        EvaluateIntelligenceIntegrityCommand cmd => _engine.ExecuteAsync(cmd, context, cancellationToken),
        _ => Task.FromResult(EngineResult.Fail(
            $"Unknown command: {command.GetType().Name}", "UNKNOWN_COMMAND"))
    };
}
