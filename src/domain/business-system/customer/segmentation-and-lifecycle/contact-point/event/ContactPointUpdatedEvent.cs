using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ContactPointId ContactPointId,
    ContactPointValue Value) : DomainEvent;
