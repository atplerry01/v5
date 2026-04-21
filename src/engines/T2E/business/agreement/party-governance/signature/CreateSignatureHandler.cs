using Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Signature;

public sealed class CreateSignatureHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSignatureCommand cmd)
            return Task.CompletedTask;

        var aggregate = SignatureAggregate.Create(new SignatureId(cmd.SignatureId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
