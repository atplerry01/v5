using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class ChangeLifecycleStageHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ChangeLifecycleStageCommand cmd)
            return;

        if (!Enum.TryParse<LifecycleStage>(cmd.To, ignoreCase: true, out var to))
            throw new InvalidOperationException(
                $"Unknown LifecycleStage '{cmd.To}'.");

        var aggregate = (LifecycleAggregate)await context.LoadAggregateAsync(typeof(LifecycleAggregate));
        aggregate.ChangeStage(to, cmd.ChangedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
