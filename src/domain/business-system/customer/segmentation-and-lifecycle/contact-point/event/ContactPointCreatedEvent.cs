using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointCreatedEvent(
    ContactPointId ContactPointId,
    CustomerRef Customer,
    ContactPointKind Kind,
    ContactPointValue Value);
