namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public static class ContactPointErrors
{
    public static ContactPointDomainException MissingId()
        => new("ContactPointId is required and must not be empty.");

    public static ContactPointDomainException MissingCustomerRef()
        => new("ContactPoint must reference a customer.");

    public static ContactPointDomainException InvalidStateTransition(ContactPointStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ContactPointDomainException ArchivedImmutable(ContactPointId id)
        => new($"ContactPoint '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ContactPointDomainException : Exception
{
    public ContactPointDomainException(string message) : base(message) { }
}
