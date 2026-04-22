using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using DomainEvents = Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ConstitutionalChainAnchorRecordSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AnchorRecordCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.AnchorRecordCreatedEvent), typeof(AnchorRecordCreatedEventSchema));
        sink.RegisterSchema("AnchorRecordSealedEvent", EventVersion.Default,
            typeof(DomainEvents.AnchorRecordSealedEvent), typeof(AnchorRecordSealedEventSchema));

        sink.RegisterPayloadMapper("AnchorRecordCreatedEvent", e =>
        {
            var evt = (DomainEvents.AnchorRecordCreatedEvent)e;
            return new AnchorRecordCreatedEventSchema(
                evt.AnchorRecordId.Value,
                evt.Descriptor.CorrelationId,
                evt.Descriptor.BlockHash,
                evt.Descriptor.EventHash,
                evt.Descriptor.PreviousBlockHash,
                evt.Descriptor.DecisionHash,
                evt.Descriptor.Sequence,
                evt.AnchoredAt);
        });

        sink.RegisterPayloadMapper("AnchorRecordSealedEvent", e =>
        {
            var evt = (DomainEvents.AnchorRecordSealedEvent)e;
            return new AnchorRecordSealedEventSchema(
                evt.AnchorRecordId.Value,
                evt.SealedAt);
        });
    }
}
