using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public sealed record AnchorRecordCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AnchorRecordId AnchorRecordId,
    AnchorDescriptor Descriptor,
    DateTimeOffset AnchoredAt) : DomainEvent;
