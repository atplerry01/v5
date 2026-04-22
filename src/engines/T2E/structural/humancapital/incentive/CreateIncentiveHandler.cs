using Whycespace.Domain.BusinessSystem.Workforce.Incentive;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Incentive;

public sealed class CreateIncentiveHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateIncentiveCommand cmd) return Task.CompletedTask;
        var aggregate = IncentiveAggregate.Create(
            new IncentiveId(cmd.IncentiveId),
            new IncentiveDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
