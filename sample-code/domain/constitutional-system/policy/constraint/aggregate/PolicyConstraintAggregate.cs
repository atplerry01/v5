namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyConstraintAggregate : AggregateRoot
{
    public Guid PolicyRuleId { get; private set; }
    public ConstraintExpression Expression { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private PolicyConstraintAggregate() { }

    public static PolicyConstraintAggregate Create(
        Guid constraintId,
        Guid policyRuleId,
        string name,
        string description,
        ConstraintExpression expression,
        DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(expression);

        var constraint = new PolicyConstraintAggregate
        {
            Id = constraintId,
            PolicyRuleId = policyRuleId,
            Name = name,
            Description = description,
            Expression = expression,
            IsActive = true,
            CreatedAt = timestamp
        };

        constraint.RaiseDomainEvent(new ConstraintCreatedEvent(constraint.Id, policyRuleId, name));
        return constraint;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new ConstraintDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void UpdateExpression(ConstraintExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        Expression = expression;
    }
}
