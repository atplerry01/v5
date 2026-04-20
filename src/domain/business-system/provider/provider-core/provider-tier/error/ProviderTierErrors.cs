namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public static class ProviderTierErrors
{
    public static ProviderTierDomainException MissingId()
        => new("ProviderTierId is required and must not be empty.");

    public static ProviderTierDomainException InvalidStateTransition(ProviderTierStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderTierDomainException ArchivedImmutable(ProviderTierId id)
        => new($"ProviderTier '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ProviderTierDomainException : Exception
{
    public ProviderTierDomainException(string message) : base(message) { }
}
