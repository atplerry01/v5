namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CommandRoutingStatus status)
    {
        return status == CommandRoutingStatus.Defined;
    }
}
