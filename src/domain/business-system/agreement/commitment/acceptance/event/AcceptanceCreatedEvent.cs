using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public sealed record AcceptanceCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AcceptanceId AcceptanceId) : DomainEvent;
