using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public sealed record EvidenceRecordCreatedEvent(
    [property: JsonPropertyName("AggregateId")] EvidenceRecordId EvidenceRecordId,
    EvidenceDescriptor Descriptor,
    DateTimeOffset RecordedAt) : DomainEvent;
