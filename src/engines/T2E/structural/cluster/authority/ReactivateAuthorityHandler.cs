using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

public sealed class ReactivateAuthorityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReactivateAuthorityCommand) return;
        var aggregate = (AuthorityAggregate)await context.LoadAggregateAsync(typeof(AuthorityAggregate));
        aggregate.Reactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
