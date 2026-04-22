using Whycespace.Domain.TrustSystem.Identity.Consent;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public sealed class RevokeConsentHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public RevokeConsentHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeConsentCommand)
            return;

        var aggregate = (ConsentAggregate)await context.LoadAggregateAsync(typeof(ConsentAggregate));
        aggregate.Revoke();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordConsentRevoked(aggregate.Descriptor.ConsentScope);
    }
}
