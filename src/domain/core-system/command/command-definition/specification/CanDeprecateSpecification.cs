namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(CommandDefinitionStatus status)
    {
        return status == CommandDefinitionStatus.Published;
    }
}
