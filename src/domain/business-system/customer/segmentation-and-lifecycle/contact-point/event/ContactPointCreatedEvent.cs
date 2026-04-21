using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ContactPointId ContactPointId,
    CustomerRef Customer,
    ContactPointKind Kind,
    ContactPointValue Value) : DomainEvent;
