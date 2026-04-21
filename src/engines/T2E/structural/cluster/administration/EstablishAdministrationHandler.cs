using Whycespace.Domain.StructuralSystem.Cluster.Administration;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Engines.T2E.Structural.Cluster.Administration;

public sealed class EstablishAdministrationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EstablishAdministrationCommand cmd) return Task.CompletedTask;
        var aggregate = AdministrationAggregate.Establish(
            new AdministrationId(cmd.AdministrationId),
            new AdministrationDescriptor(new ClusterRef(cmd.ClusterReference), cmd.AdministrationName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
