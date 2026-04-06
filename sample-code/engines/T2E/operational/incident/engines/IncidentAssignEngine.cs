using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentAssignEngine : IEngine<AssignIncidentCommand>
{
    public Task<EngineResult> ExecuteAsync(
        AssignIncidentCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var @event = new IncidentAssignedEventDto
        {
            AggregateId = command.AggregateId,
            AssigneeIdentityId = command.AssigneeIdentityId,
            EscalationLevel = command.EscalationLevel
        };

        return Task.FromResult(EngineResult.Ok(@event));
    }
}
