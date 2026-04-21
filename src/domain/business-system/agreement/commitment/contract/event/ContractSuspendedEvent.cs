using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed record ContractSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] ContractId ContractId) : DomainEvent;
