using Whycespace.Shared.Contracts.Domain.Operational.Incident;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentCommandHandler
{
    private readonly IncidentCreateEngine _createEngine;
    private readonly IncidentAssignEngine _assignEngine = new();
    private readonly IncidentEscalateEngine _escalateEngine = new();
    private readonly IncidentResolveEngine _resolveEngine = new();
    private readonly IncidentCloseEngine _closeEngine = new();

    public IncidentCommandHandler(IIncidentDomainService incidentDomainService)
    {
        _createEngine = new IncidentCreateEngine(incidentDomainService);
    }

    public Task<EngineResult> HandleAsync(
        IncidentCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateIncidentCommand create => _createEngine.ExecuteAsync(create, context, ct),
        AssignIncidentCommand assign => _assignEngine.ExecuteAsync(assign, context, ct),
        EscalateIncidentCommand escalate => _escalateEngine.ExecuteAsync(escalate, context, ct),
        ResolveIncidentCommand resolve => _resolveEngine.ExecuteAsync(resolve, context, ct),
        CloseIncidentCommand close => _closeEngine.ExecuteAsync(close, context, ct),
        _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
