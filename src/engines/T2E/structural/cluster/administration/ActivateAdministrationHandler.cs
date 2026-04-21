using Whycespace.Domain.StructuralSystem.Cluster.Administration;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Engines.T2E.Structural.Cluster.Administration;

public sealed class ActivateAdministrationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateAdministrationCommand) return;
        var aggregate = (AdministrationAggregate)await context.LoadAggregateAsync(typeof(AdministrationAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
