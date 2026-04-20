namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointUpdatedEvent(ContactPointId ContactPointId, ContactPointValue Value);
