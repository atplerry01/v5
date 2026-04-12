namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(EventDefinitionStatus status)
    {
        return status == EventDefinitionStatus.Published;
    }
}
