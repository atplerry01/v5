namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointPreferredSetEvent(ContactPointId ContactPointId, bool IsPreferred);
