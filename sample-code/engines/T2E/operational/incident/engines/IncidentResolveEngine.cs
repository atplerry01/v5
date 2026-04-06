using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Incident;

public sealed class IncidentResolveEngine : IEngine<ResolveIncidentCommand>
{
    public Task<EngineResult> ExecuteAsync(
        ResolveIncidentCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var @event = new IncidentResolvedEventDto
        {
            AggregateId = command.AggregateId
        };

        return Task.FromResult(EngineResult.Ok(@event));
    }
}
