using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public sealed record EvidenceRecordArchivedEvent(
    [property: JsonPropertyName("AggregateId")] EvidenceRecordId EvidenceRecordId,
    DateTimeOffset ArchivedAt) : DomainEvent;
