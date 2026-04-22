using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyEnforcement;

public sealed class RecordPolicyEnforcementHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordPolicyEnforcementCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyEnforcementAggregate.Record(
            new PolicyEnforcementId(cmd.EnforcementId.ToString("N").PadRight(64, '0')),
            cmd.PolicyDecisionId,
            cmd.TargetId,
            Enum.Parse<PolicyEnforcementOutcome>(cmd.Outcome, ignoreCase: true),
            cmd.EnforcedAt,
            cmd.IsNoPolicyFlagAnomaly);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
