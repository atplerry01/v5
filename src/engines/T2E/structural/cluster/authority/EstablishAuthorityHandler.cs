using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

public sealed class EstablishAuthorityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EstablishAuthorityCommand cmd) return Task.CompletedTask;
        var aggregate = AuthorityAggregate.Establish(
            new AuthorityId(cmd.AuthorityId),
            new AuthorityDescriptor(new ClusterRef(cmd.ClusterReference), cmd.AuthorityName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
