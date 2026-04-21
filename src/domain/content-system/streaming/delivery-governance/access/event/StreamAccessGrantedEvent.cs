using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessGrantedEvent(
    [property: JsonPropertyName("AggregateId")] StreamAccessId AccessId,
    StreamRef StreamRef,
    AccessMode Mode,
    AccessWindow Window,
    TokenBinding Token,
    Timestamp GrantedAt) : DomainEvent;
