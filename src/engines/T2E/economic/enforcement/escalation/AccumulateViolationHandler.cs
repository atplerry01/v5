using Whycespace.Domain.EconomicSystem.Enforcement.Escalation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Escalation;

public sealed class AccumulateViolationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AccumulateViolationCommand cmd) return;

        var loaded = await context.LoadAggregateAsync(typeof(ViolationEscalationAggregate));
        var existing = loaded as ViolationEscalationAggregate;
        if (existing is not null && existing.Version < 0) existing = null;

        var severityWeight = EscalationThresholdSpecification.WeightFor(cmd.Severity);
        if (severityWeight == 0)
            throw EscalationErrors.UnknownSeverity(cmd.Severity);

        var aggregate = ViolationEscalationAggregate.Accumulate(
            existing,
            new SubjectId(cmd.SubjectId),
            cmd.ViolationId,
            severityWeight,
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
