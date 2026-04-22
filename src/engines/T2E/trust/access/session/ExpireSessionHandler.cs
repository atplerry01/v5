using Whycespace.Domain.TrustSystem.Access.Session;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Engines.T2E.Trust.Access.Session;

public sealed class ExpireSessionHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public ExpireSessionHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireSessionCommand)
            return;

        var aggregate = (SessionAggregate)await context.LoadAggregateAsync(typeof(SessionAggregate));
        aggregate.Expire();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordSessionExpired(aggregate.Descriptor.SessionContext);
    }
}
