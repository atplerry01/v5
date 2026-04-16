using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Execution;

/// <summary>
/// Final gate before command dispatch to engine.
/// Marks the command context as runtime-originated (RuntimeOriginKey).
/// Ensures all preceding guards and policy evaluation have passed.
/// No command reaches an engine without this guard approving it.
///
/// Phase-4 enforcement integration: when an <see cref="IViolationStateQuery"/>
/// is wired, the guard consults the active-violation projection for the
/// resolved IdentityId and either (a) rejects the command if the subject
/// has any active Block violation, or (b) stamps
/// <see cref="CommandContext.EnforcementConstraint"/> with the active
/// Restrict / Warn / Escalate action so engine handlers can degrade
/// behavior. The query is optional — hosts without enforcement wiring
/// retain the pre-phase-4 behavior verbatim.
/// </summary>
public sealed class ExecutionGuardMiddleware : IMiddleware
{
    public const string RuntimeOriginKey = "Runtime.IsFromRuntime";

    private readonly IViolationStateQuery? _violationStateQuery;
    private readonly IEscalationStateQuery? _escalationStateQuery;

    public ExecutionGuardMiddleware()
    {
        _violationStateQuery = null;
        _escalationStateQuery = null;
    }

    public ExecutionGuardMiddleware(IViolationStateQuery? violationStateQuery)
        : this(violationStateQuery, null) { }

    public ExecutionGuardMiddleware(
        IViolationStateQuery? violationStateQuery,
        IEscalationStateQuery? escalationStateQuery)
    {
        _violationStateQuery = violationStateQuery;
        _escalationStateQuery = escalationStateQuery;
    }

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        // Final validation: policy decision must have been evaluated
        if (context.PolicyDecisionAllowed is not true)
        {
            return CommandResult.Failure(
                "Execution guard: command cannot proceed without approved policy decision.");
        }

        if (string.IsNullOrWhiteSpace(context.PolicyDecisionHash))
        {
            return CommandResult.Failure(
                "Execution guard: policy decision hash is required for chain anchoring.");
        }

        var hasSubject = Guid.TryParse(context.IdentityId, out var subjectId) && subjectId != Guid.Empty;

        if (_violationStateQuery is not null && hasSubject)
        {
            var posture = await _violationStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);
            if (posture.IsBlocked)
            {
                return CommandResult.Failure(
                    "Execution guard: subject has an active Critical+Block enforcement violation. Command rejected.");
            }
            if (!string.IsNullOrEmpty(posture.Constraint))
            {
                context.EnforcementConstraint = posture.Constraint;
            }
        }

        if (_escalationStateQuery is not null && hasSubject)
        {
            var escalation = await _escalationStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);
            if (escalation.IsCritical)
            {
                return CommandResult.Failure(
                    "Execution guard: subject escalation level is Critical. Command rejected.");
            }
            if (escalation.IsHigh || escalation.IsMedium)
            {
                // Stamp only when a stronger constraint from the violation layer
                // has not already been set; High overrides Medium.
                if (string.IsNullOrEmpty(context.EnforcementConstraint)
                    || string.Equals(context.EnforcementConstraint, "Medium", StringComparison.Ordinal))
                {
                    context.EnforcementConstraint = escalation.Level;
                }
            }
        }

        // Mark context as runtime-originated — engines can verify this
        context.RuntimeOrigin = true;

        return await next(cancellationToken);
    }
}
