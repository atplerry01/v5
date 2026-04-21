using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessRevokedEvent(
    [property: JsonPropertyName("AggregateId")] StreamAccessId AccessId,
    string Reason,
    Timestamp RevokedAt) : DomainEvent;
