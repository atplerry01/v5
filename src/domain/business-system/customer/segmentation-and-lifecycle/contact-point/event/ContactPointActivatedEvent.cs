using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ContactPointId ContactPointId) : DomainEvent;
