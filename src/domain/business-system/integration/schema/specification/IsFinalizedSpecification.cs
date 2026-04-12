namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public sealed class IsFinalizedSpecification
{
    public bool IsSatisfiedBy(SchemaStatus status)
    {
        return status == SchemaStatus.Finalized;
    }
}
