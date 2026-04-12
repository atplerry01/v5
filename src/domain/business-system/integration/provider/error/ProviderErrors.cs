namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public static class ProviderErrors
{
    public static ProviderDomainException MissingId()
        => new("ProviderId is required and must not be empty.");

    public static ProviderDomainException MissingProfile()
        => new("ProviderProfile is required and must not be null.");

    public static ProviderDomainException InvalidStateTransition(ProviderStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderDomainException AlreadyActive(ProviderId id)
        => new($"Provider '{id.Value}' is already active.");

    public static ProviderDomainException AlreadySuspended(ProviderId id)
        => new($"Provider '{id.Value}' is already suspended.");
}

public sealed class ProviderDomainException : Exception
{
    public ProviderDomainException(string message) : base(message) { }
}
