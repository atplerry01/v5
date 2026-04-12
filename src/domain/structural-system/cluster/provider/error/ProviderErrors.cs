namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public static class ProviderErrors
{
    public static InvalidOperationException MissingId()
        => new("ProviderId is required and must not be empty.");

    public static InvalidOperationException MissingProfile()
        => new("ProviderProfile is required.");

    public static InvalidOperationException InvalidStateTransition(ProviderStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
