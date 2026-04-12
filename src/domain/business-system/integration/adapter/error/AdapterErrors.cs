namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public static class AdapterErrors
{
    public static AdapterDomainException MissingId()
        => new("AdapterId is required and must not be empty.");

    public static AdapterDomainException MissingTypeId()
        => new("AdapterTypeId is required and must not be empty.");

    public static AdapterDomainException InvalidStateTransition(AdapterStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AdapterDomainException AlreadyActive(AdapterId id)
        => new($"Adapter '{id.Value}' is already active.");

    public static AdapterDomainException AlreadyDisabled(AdapterId id)
        => new($"Adapter '{id.Value}' is already disabled.");
}

public sealed class AdapterDomainException : Exception
{
    public AdapterDomainException(string message) : base(message) { }
}
