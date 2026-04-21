using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Observability;

public sealed class UpdateObservabilityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateObservabilityCommand cmd) return;

        var aggregate = (ObservabilityAggregate)await context.LoadAggregateAsync(typeof(ObservabilityAggregate));

        var newSnapshot = new ObservabilitySnapshot(
            new ViewerCount(cmd.Viewers),
            new PlaybackCount(cmd.Playbacks),
            new ErrorCount(cmd.Errors),
            new DropCount(cmd.Drops),
            new BitrateMeasurement(cmd.AverageBitrateBps),
            new LatencyMeasurement(cmd.AverageLatencyMs));

        aggregate.Update(newSnapshot, new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
