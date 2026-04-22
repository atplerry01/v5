using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyDetection;

public sealed class DetectDiscrepancyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DetectDiscrepancyCommand cmd)
            return Task.CompletedTask;

        var aggregate = DiscrepancyDetectionAggregate.Detect(
            new DiscrepancyDetectionId(cmd.DetectionId.ToString("N").PadRight(64, '0')),
            Enum.Parse<DiscrepancyKind>(cmd.Kind, ignoreCase: true),
            cmd.SourceReference,
            cmd.DetectedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
