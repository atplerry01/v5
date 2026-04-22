using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using DomainEvents = Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ConstitutionalChainLedgerSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("LedgerOpenedEvent", EventVersion.Default,
            typeof(DomainEvents.LedgerOpenedEvent), typeof(LedgerOpenedEventSchema));
        sink.RegisterSchema("LedgerSealedEvent", EventVersion.Default,
            typeof(DomainEvents.LedgerSealedEvent), typeof(LedgerSealedEventSchema));

        sink.RegisterPayloadMapper("LedgerOpenedEvent", e =>
        {
            var evt = (DomainEvents.LedgerOpenedEvent)e;
            return new LedgerOpenedEventSchema(
                evt.LedgerId.Value,
                evt.Descriptor.LedgerName,
                evt.OpenedAt);
        });

        sink.RegisterPayloadMapper("LedgerSealedEvent", e =>
        {
            var evt = (DomainEvents.LedgerSealedEvent)e;
            return new LedgerSealedEventSchema(
                evt.LedgerId.Value,
                evt.SealedAt);
        });
    }
}
