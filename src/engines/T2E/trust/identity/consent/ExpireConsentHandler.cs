using Whycespace.Domain.TrustSystem.Identity.Consent;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public sealed class ExpireConsentHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public ExpireConsentHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExpireConsentCommand)
            return;

        var aggregate = (ConsentAggregate)await context.LoadAggregateAsync(typeof(ConsentAggregate));
        aggregate.Expire();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordConsentExpired(aggregate.Descriptor.ConsentScope);
    }
}
