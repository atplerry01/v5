using Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using Whycespace.Domain.EconomicSystem.Enforcement.Violation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Violation;

public sealed class DetectViolationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DetectViolationCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<ViolationSeverity>(cmd.Severity, ignoreCase: true, out var severity))
            throw new InvalidOperationException($"Unknown violation severity: '{cmd.Severity}'.");
        if (!Enum.TryParse<EnforcementAction>(cmd.RecommendedAction, ignoreCase: true, out var action))
            throw new InvalidOperationException($"Unknown enforcement action: '{cmd.RecommendedAction}'.");

        var aggregate = ViolationAggregate.Detect(
            new ViolationId(cmd.ViolationId),
            new RuleId(cmd.RuleId),
            new SourceReference(cmd.SourceReference),
            cmd.Reason,
            severity,
            action,
            new Timestamp(cmd.DetectedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
