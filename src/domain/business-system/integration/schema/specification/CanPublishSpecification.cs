namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(SchemaStatus status)
    {
        return status == SchemaStatus.Draft;
    }
}
