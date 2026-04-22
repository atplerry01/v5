using Whycespace.Domain.ControlSystem.Enforcement.Rule;

namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public sealed class ViolationTraceService
{
    public bool HasValidRuleReference(ViolationAggregate violation) =>
        violation.RuleId.Value != Guid.Empty;

    public bool HasValidSourceReference(ViolationAggregate violation) =>
        violation.Source.Value != Guid.Empty;
}
