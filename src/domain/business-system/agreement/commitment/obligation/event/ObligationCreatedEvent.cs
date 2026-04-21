using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public sealed record ObligationCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ObligationId ObligationId) : DomainEvent;
