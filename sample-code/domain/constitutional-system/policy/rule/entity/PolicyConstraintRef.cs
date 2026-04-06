namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class PolicyConstraintRef
{
    public Guid ConstraintId { get; }
    public string Name { get; }

    private PolicyConstraintRef(Guid constraintId, string name)
    {
        ConstraintId = constraintId;
        Name = name;
    }

    public static PolicyConstraintRef From(Guid constraintId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PolicyConstraintRef(constraintId, name);
    }
}
