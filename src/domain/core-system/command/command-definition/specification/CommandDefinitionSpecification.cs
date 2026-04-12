namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(CommandDefinitionStatus status)
    {
        return status == CommandDefinitionStatus.Draft;
    }
}
