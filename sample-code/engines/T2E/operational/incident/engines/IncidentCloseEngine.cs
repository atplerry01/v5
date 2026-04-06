using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentCloseEngine : IEngine<CloseIncidentCommand>
{
    public Task<EngineResult> ExecuteAsync(
        CloseIncidentCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var @event = new IncidentClosedEventDto
        {
            AggregateId = command.AggregateId
        };

        return Task.FromResult(EngineResult.Ok(@event));
    }
}
