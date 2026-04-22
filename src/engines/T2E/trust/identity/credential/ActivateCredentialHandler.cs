using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public sealed class ActivateCredentialHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateCredentialCommand cmd)
            return;

        var aggregate = (CredentialAggregate)await context.LoadAggregateAsync(typeof(CredentialAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
