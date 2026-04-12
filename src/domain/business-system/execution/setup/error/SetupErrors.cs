namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public static class SetupErrors
{
    public static SetupDomainException MissingId()
        => new("SetupId is required and must not be empty.");

    public static SetupDomainException MissingTargetId()
        => new("SetupTargetId is required and must not be empty.");

    public static SetupDomainException AlreadyConfigured()
        => new("Setup has already been configured.");

    public static SetupDomainException AlreadyReady()
        => new("Setup is already marked as ready.");

    public static SetupDomainException InvalidStateTransition(SetupStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SetupDomainException : Exception
{
    public SetupDomainException(string message) : base(message) { }
}
