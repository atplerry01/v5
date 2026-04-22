using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public sealed record ContractDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] ContractId ContractId,
    Timestamp DeprecatedAt) : DomainEvent;
