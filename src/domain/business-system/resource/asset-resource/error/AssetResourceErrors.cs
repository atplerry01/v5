namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public static class AssetResourceErrors
{
    public static AssetResourceDomainException MissingId()
        => new("AssetResourceId is required and must not be empty.");

    public static AssetResourceDomainException AlreadyActive(AssetResourceId id)
        => new($"AssetResource '{id.Value}' is already active.");

    public static AssetResourceDomainException AlreadyDecommissioned(AssetResourceId id)
        => new($"AssetResource '{id.Value}' has already been decommissioned.");

    public static AssetResourceDomainException InvalidStateTransition(AssetResourceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class AssetResourceDomainException : Exception
{
    public AssetResourceDomainException(string message) : base(message) { }
}
