using Whycespace.Domain.ControlSystem.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemMetric;

public sealed class DeprecateSystemMetricHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateSystemMetricCommand)
            return;

        var aggregate = (SystemMetricAggregate)await context.LoadAggregateAsync(typeof(SystemMetricAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
