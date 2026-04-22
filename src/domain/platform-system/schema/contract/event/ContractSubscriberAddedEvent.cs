using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public sealed record ContractSubscriberAddedEvent(
    [property: JsonPropertyName("AggregateId")] ContractId ContractId,
    SubscriberConstraint Constraint,
    Timestamp AddedAt) : DomainEvent;
