using Whycespace.Domain.ControlSystem.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemHealth;

public sealed class RecordSystemHealthDegradationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordSystemHealthDegradationCommand cmd)
            return;

        var aggregate = (SystemHealthAggregate)await context.LoadAggregateAsync(typeof(SystemHealthAggregate));
        aggregate.RecordDegradation(
            Enum.Parse<HealthStatus>(cmd.NewStatus, ignoreCase: true),
            cmd.Reason,
            cmd.OccurredAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
