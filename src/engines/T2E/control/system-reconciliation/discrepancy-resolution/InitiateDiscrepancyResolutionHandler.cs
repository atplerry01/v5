using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyResolution;

public sealed class InitiateDiscrepancyResolutionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateDiscrepancyResolutionCommand cmd)
            return Task.CompletedTask;

        var aggregate = DiscrepancyResolutionAggregate.Initiate(
            new DiscrepancyResolutionId(cmd.ResolutionId.ToString("N").PadRight(64, '0')),
            new DiscrepancyDetectionId(cmd.DetectionId),
            cmd.InitiatedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
