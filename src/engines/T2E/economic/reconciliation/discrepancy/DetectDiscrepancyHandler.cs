using Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Reconciliation.Discrepancy;

public sealed class DetectDiscrepancyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DetectDiscrepancyCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<DiscrepancySource>(cmd.Source, ignoreCase: true, out var source))
            throw new ArgumentException(
                $"Unknown DiscrepancySource value: '{cmd.Source}'.", nameof(cmd.Source));

        var aggregate = DiscrepancyAggregate.Detect(
            new DiscrepancyId(cmd.DiscrepancyId),
            new ProcessReference(cmd.ProcessReference),
            source,
            new Amount(cmd.ExpectedValue),
            new Amount(cmd.ActualValue),
            new Amount(cmd.Difference),
            new Timestamp(cmd.DetectedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
