using Whycespace.Domain.EconomicSystem.Enforcement.Rule;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Violation;

public sealed class ViolationTraceService
{
    public bool HasValidRuleReference(ViolationAggregate violation) =>
        violation.RuleId.Value != Guid.Empty;

    public bool HasValidSourceReference(ViolationAggregate violation) =>
        violation.Source.Value != Guid.Empty;
}
