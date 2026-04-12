namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(CommandRoutingStatus status)
    {
        return status == CommandRoutingStatus.Active;
    }
}
