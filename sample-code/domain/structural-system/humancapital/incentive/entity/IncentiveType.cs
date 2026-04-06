namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed class IncentiveType
{
    public Guid Id { get; }
    public string TypeName { get; }

    public IncentiveType(Guid id, string typeName)
    {
        Id = id;
        TypeName = typeName;
    }
}
