namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public static class FacilityErrors
{
    public static FacilityDomainException MissingId()
        => new("FacilityId is required and must not be empty.");

    public static FacilityDomainException AlreadyActive(FacilityId id)
        => new($"Facility '{id.Value}' is already active.");

    public static FacilityDomainException AlreadyClosed(FacilityId id)
        => new($"Facility '{id.Value}' has already been closed.");

    public static FacilityDomainException InvalidStateTransition(FacilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class FacilityDomainException : Exception
{
    public FacilityDomainException(string message) : base(message) { }
}
