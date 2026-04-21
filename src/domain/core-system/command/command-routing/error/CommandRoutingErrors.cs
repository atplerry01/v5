using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public static class CommandRoutingErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CommandRoutingId is required and must not be empty.");

    public static DomainException MissingRoutingRule()
        => new DomainInvariantViolationException("Command routing must include a valid routing rule.");

    public static DomainException InvalidStateTransition(CommandRoutingStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("CommandRouting has already been initialized.");
}
