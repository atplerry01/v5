namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public static class ProviderErrors
{
    public static ProviderDomainException MissingId()
        => new("ProviderId is required and must not be empty.");

    public static ProviderDomainException InvalidStateTransition(ProviderStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderDomainException ArchivedImmutable(ProviderId id)
        => new($"Provider '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ProviderDomainException : Exception
{
    public ProviderDomainException(string message) : base(message) { }
}
