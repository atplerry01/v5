using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using DomainEvents = Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ConstitutionalChainEvidenceRecordSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EvidenceRecordCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.EvidenceRecordCreatedEvent), typeof(EvidenceRecordCreatedEventSchema));
        sink.RegisterSchema("EvidenceRecordArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.EvidenceRecordArchivedEvent), typeof(EvidenceRecordArchivedEventSchema));

        sink.RegisterPayloadMapper("EvidenceRecordCreatedEvent", e =>
        {
            var evt = (DomainEvents.EvidenceRecordCreatedEvent)e;
            return new EvidenceRecordCreatedEventSchema(
                evt.EvidenceRecordId.Value,
                evt.Descriptor.CorrelationId,
                evt.Descriptor.AnchorRecordId,
                evt.Descriptor.EvidenceType.ToString(),
                evt.Descriptor.ActorId,
                evt.Descriptor.SubjectId,
                evt.Descriptor.PolicyHash,
                evt.RecordedAt);
        });

        sink.RegisterPayloadMapper("EvidenceRecordArchivedEvent", e =>
        {
            var evt = (DomainEvents.EvidenceRecordArchivedEvent)e;
            return new EvidenceRecordArchivedEventSchema(
                evt.EvidenceRecordId.Value,
                evt.ArchivedAt);
        });
    }
}
