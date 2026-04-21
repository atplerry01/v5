using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class StartLifecycleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartLifecycleCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<LifecycleStage>(cmd.InitialStage, ignoreCase: true, out var stage))
            throw new InvalidOperationException(
                $"Unknown LifecycleStage '{cmd.InitialStage}'.");

        var aggregate = LifecycleAggregate.Start(
            new LifecycleId(cmd.LifecycleId),
            new CustomerRef(cmd.CustomerId),
            stage,
            cmd.StartedAt);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
