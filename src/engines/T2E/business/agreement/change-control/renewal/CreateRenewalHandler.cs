using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Renewal;

public sealed class CreateRenewalHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateRenewalCommand cmd)
            return Task.CompletedTask;

        var aggregate = RenewalAggregate.Create(
            new RenewalId(cmd.RenewalId),
            new RenewalSourceId(cmd.SourceId));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
