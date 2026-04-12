namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public static class SecretErrors
{
    public static InvalidOperationException MissingId()
        => new("SecretId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("SecretDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(SecretStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
