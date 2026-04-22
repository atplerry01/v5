using Whycespace.Domain.ControlSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public sealed record ViolationDetectedEvent(
    ViolationId ViolationId,
    RuleId RuleId,
    SourceReference Source,
    string Reason,
    ViolationSeverity Severity,
    EnforcementAction RecommendedAction,
    Timestamp DetectedAt) : DomainEvent;
