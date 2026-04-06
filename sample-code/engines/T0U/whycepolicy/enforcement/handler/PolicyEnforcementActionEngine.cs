using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Contracts.Policy;
using PolicyEvaluationResult = Whycespace.Shared.Contracts.Policy.PolicyEvaluationResult;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T0U.WhycePolicy.Enforcement;

/// <summary>
/// Consumes policy evaluation results with violations and generates EnforcementActions.
/// Deterministic, idempotent — same violation always produces same action ID.
/// NO external calls. NO system mutation. Pure evaluation.
/// Uses shared contracts instead of domain aggregates.
/// </summary>
public sealed class PolicyEnforcementActionEngine
{
    private readonly IPolicyViolationMapping _violationMapping;

    public PolicyEnforcementActionEngine(IPolicyViolationMapping violationMapping)
    {
        _violationMapping = violationMapping ?? throw new ArgumentNullException(nameof(violationMapping));
    }

    public IReadOnlyList<EnforcementActionDto> Evaluate(
        PolicyEvaluationResult result,
        string correlationId,
        string classification = "system")
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        if (result.Violations.Count == 0)
            return [];

        var severity = result.IsDenied ? "critical" : "warning";
        var (enforcementType, enforcementSeverity) = _violationMapping.Resolve(severity);
        var targetType = _violationMapping.ResolveTarget(classification);

        var actions = new List<EnforcementActionDto>();

        foreach (var violation in result.Violations)
        {
            var targetId = result.EventPayload?.ActorId.ToString() ?? "unknown";

            var actionId = EnforcementIdGenerator.Generate(
                correlationId, targetId, enforcementType, violation);

            var action = new EnforcementActionDto(
                actionId,
                enforcementType,
                enforcementSeverity,
                targetType,
                targetId,
                violation,
                correlationId);

            actions.Add(action);
        }

        // Critical severity always triggers an audit action
        if (_violationMapping.IsCritical(enforcementSeverity))
        {
            var auditId = EnforcementIdGenerator.Generate(
                correlationId, "system", "AuditTrigger", "critical_violation");

            actions.Add(new EnforcementActionDto(
                auditId,
                "AuditTrigger",
                "Critical",
                "System",
                "governance",
                $"Critical policy violation — audit required. Correlation: {correlationId}",
                correlationId));
        }

        return actions;
    }
}

/// <summary>
/// Engine-local enforcement action DTO — decoupled from domain EnforcementAction aggregate.
/// </summary>
public sealed record EnforcementActionDto(
    Guid Id,
    string ActionType,
    string Severity,
    string TargetType,
    string TargetId,
    string Reason,
    string CorrelationId);
