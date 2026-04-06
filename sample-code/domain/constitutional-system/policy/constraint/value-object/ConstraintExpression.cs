namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed class ConstraintExpression : ValueObject
{
    public ConstraintOperand Left { get; }
    public ConstraintOperator Operator { get; }
    public ConstraintOperand Right { get; }

    public ConstraintExpression(ConstraintOperand left, ConstraintOperator @operator, ConstraintOperand right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(right);

        Left = left;
        Operator = @operator;
        Right = right;
    }

    public bool Evaluate(IReadOnlyDictionary<string, object> facts)
    {
        var leftValue = Left.Resolve(facts);
        var rightValue = Right.Resolve(facts);

        return Operator.Apply(leftValue, rightValue);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Left;
        yield return Operator;
        yield return Right;
    }
}
