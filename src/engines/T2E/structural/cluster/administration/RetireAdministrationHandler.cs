using Whycespace.Domain.StructuralSystem.Cluster.Administration;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Engines.T2E.Structural.Cluster.Administration;

public sealed class RetireAdministrationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireAdministrationCommand) return;
        var aggregate = (AdministrationAggregate)await context.LoadAggregateAsync(typeof(AdministrationAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
