using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentEscalateEngine : IEngine<EscalateIncidentCommand>
{
    public Task<EngineResult> ExecuteAsync(
        EscalateIncidentCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var @event = new IncidentEscalatedEventDto
        {
            AggregateId = command.AggregateId,
            PreviousSeverity = "current",
            NewSeverity = "escalated"
        };

        return Task.FromResult(EngineResult.Ok(@event));
    }
}
