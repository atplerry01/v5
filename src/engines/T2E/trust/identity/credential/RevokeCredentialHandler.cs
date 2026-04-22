using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public sealed class RevokeCredentialHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public RevokeCredentialHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeCredentialCommand)
            return;

        var aggregate = (CredentialAggregate)await context.LoadAggregateAsync(typeof(CredentialAggregate));
        aggregate.Revoke();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordCredentialRevoked(aggregate.Descriptor.CredentialType);
    }
}
