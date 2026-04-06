namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed class AssignmentScope
{
    public Guid Id { get; }
    public string ScopeType { get; }

    public AssignmentScope(Guid id, string scopeType)
    {
        Id = id;
        ScopeType = scopeType;
    }
}
