using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Operational.Incident;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentCreateEngine : IEngine<CreateIncidentCommand>
{
    private readonly IIncidentDomainService _incidentDomainService;

    public IncidentCreateEngine(IIncidentDomainService incidentDomainService)
    {
        _incidentDomainService = incidentDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(
        CreateIncidentCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "operational.incident",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _incidentDomainService.CreateAsync(
            execCtx,
            command.AggregateId,
            command.AggregateId,
            command.Description,
            command.IncidentType,
            command.Severity,
            command.Source,
            command.ReferenceDomain,
            command.SourceCorrelationId);

        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
