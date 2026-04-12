namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(EventDefinitionStatus status)
    {
        return status == EventDefinitionStatus.Draft;
    }
}
