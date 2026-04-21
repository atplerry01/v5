using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Validity;

public sealed class CreateValidityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateValidityCommand cmd)
            return Task.CompletedTask;

        var aggregate = ValidityAggregate.Create(new ValidityId(cmd.ValidityId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
