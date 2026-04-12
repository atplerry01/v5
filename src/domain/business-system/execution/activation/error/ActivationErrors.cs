namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public static class ActivationErrors
{
    public static ActivationDomainException MissingId()
        => new("ActivationId is required and must not be empty.");

    public static ActivationDomainException MissingTargetId()
        => new("ActivationTargetId is required and must not be empty.");

    public static ActivationDomainException InvalidStateTransition(ActivationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ActivationDomainException AlreadyActive(ActivationId id)
        => new($"Activation '{id.Value}' is already active.");

    public static ActivationDomainException AlreadyDeactivated(ActivationId id)
        => new($"Activation '{id.Value}' has already been deactivated.");
}

public sealed class ActivationDomainException : Exception
{
    public ActivationDomainException(string message) : base(message) { }
}
