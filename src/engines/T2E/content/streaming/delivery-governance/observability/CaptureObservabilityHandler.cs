using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Observability;

public sealed class CaptureObservabilityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CaptureObservabilityCommand cmd) return Task.CompletedTask;

        ArchiveRef? archiveRef = cmd.ArchiveId.HasValue ? new ArchiveRef(cmd.ArchiveId.Value) : null;

        var snapshot = new ObservabilitySnapshot(
            new ViewerCount(cmd.Viewers),
            new PlaybackCount(cmd.Playbacks),
            new ErrorCount(cmd.Errors),
            new DropCount(cmd.Drops),
            new BitrateMeasurement(cmd.AverageBitrateBps),
            new LatencyMeasurement(cmd.AverageLatencyMs));

        var aggregate = ObservabilityAggregate.Capture(
            new ObservabilityId(cmd.ObservabilityId),
            new StreamRef(cmd.StreamId),
            archiveRef,
            new ObservabilityWindow(new Timestamp(cmd.WindowStart), new Timestamp(cmd.WindowEnd)),
            snapshot,
            new Timestamp(cmd.CapturedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
