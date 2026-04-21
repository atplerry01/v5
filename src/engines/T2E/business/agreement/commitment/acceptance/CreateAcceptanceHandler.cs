using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Acceptance;

public sealed class CreateAcceptanceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAcceptanceCommand cmd)
            return Task.CompletedTask;

        var aggregate = AcceptanceAggregate.Create(new AcceptanceId(cmd.AcceptanceId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
