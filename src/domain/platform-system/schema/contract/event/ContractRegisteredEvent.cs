using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public sealed record ContractRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] ContractId ContractId,
    string ContractName,
    DomainRoute PublisherRoute,
    Guid SchemaRef,
    int SchemaVersion,
    Timestamp RegisteredAt) : DomainEvent;
