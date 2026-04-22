using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public sealed record AnchorRecordSealedEvent(
    [property: JsonPropertyName("AggregateId")] AnchorRecordId AnchorRecordId,
    DateTimeOffset SealedAt) : DomainEvent;
