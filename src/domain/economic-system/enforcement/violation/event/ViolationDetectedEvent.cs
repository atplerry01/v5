using Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Violation;

public sealed record ViolationDetectedEvent(
    ViolationId ViolationId,
    RuleId RuleId,
    SourceReference Source,
    string Reason,
    ViolationSeverity Severity,
    EnforcementAction RecommendedAction,
    Timestamp DetectedAt) : DomainEvent;
