using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessRestrictedEvent(
    [property: JsonPropertyName("AggregateId")] StreamAccessId AccessId,
    string Reason,
    Timestamp RestrictedAt) : DomainEvent;
