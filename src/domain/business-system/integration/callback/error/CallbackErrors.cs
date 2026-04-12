namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public static class CallbackErrors
{
    public static CallbackDomainException MissingId()
        => new("CallbackId is required and must not be empty.");

    public static CallbackDomainException MissingDefinition()
        => new("CallbackDefinition is required and must not be null.");

    public static CallbackDomainException InvalidStateTransition(CallbackStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CallbackDomainException AlreadyPending(CallbackId id)
        => new($"Callback '{id.Value}' is already pending.");

    public static CallbackDomainException AlreadyCompleted(CallbackId id)
        => new($"Callback '{id.Value}' has already been completed.");
}

public sealed class CallbackDomainException : Exception
{
    public CallbackDomainException(string message) : base(message) { }
}
