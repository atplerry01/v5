using Whycespace.Domain.TrustSystem.Access.Session;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Engines.T2E.Trust.Access.Session;

public sealed class TerminateSessionHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public TerminateSessionHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TerminateSessionCommand)
            return;

        var aggregate = (SessionAggregate)await context.LoadAggregateAsync(typeof(SessionAggregate));
        aggregate.Terminate();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordSessionTerminated(aggregate.Descriptor.SessionContext);
    }
}
