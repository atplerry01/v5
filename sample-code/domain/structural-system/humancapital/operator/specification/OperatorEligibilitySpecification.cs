namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed class OperatorEligibilitySpecification
{
    public bool IsSatisfiedBy(OperatorAggregate op) => op.IsAuthorized;
}
