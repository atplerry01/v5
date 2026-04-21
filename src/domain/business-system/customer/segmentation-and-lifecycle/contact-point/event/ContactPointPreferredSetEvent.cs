using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record ContactPointPreferredSetEvent(
    [property: JsonPropertyName("AggregateId")] ContactPointId ContactPointId,
    bool IsPreferred) : DomainEvent;
