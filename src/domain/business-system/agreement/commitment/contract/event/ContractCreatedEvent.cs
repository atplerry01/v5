using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed record ContractCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ContractId ContractId,
    DateTimeOffset CreatedAt) : DomainEvent;
