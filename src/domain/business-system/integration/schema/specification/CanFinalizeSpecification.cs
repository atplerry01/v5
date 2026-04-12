namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(SchemaStatus status)
    {
        return status == SchemaStatus.Published;
    }
}
