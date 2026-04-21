using Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Signature;

public sealed class SignSignatureHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SignSignatureCommand)
            return;

        var aggregate = (SignatureAggregate)await context.LoadAggregateAsync(typeof(SignatureAggregate));
        aggregate.Sign();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
