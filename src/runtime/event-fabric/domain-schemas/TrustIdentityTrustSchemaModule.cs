using Whycespace.Shared.Contracts.Events.Trust.Identity.Trust;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Trust;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityTrustSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("TrustAssessedEvent", EventVersion.Default,
            typeof(DomainEvents.TrustAssessedEvent), typeof(TrustAssessedEventSchema));
        sink.RegisterSchema("TrustActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.TrustActivatedEvent), typeof(TrustActivatedEventSchema));
        sink.RegisterSchema("TrustSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.TrustSuspendedEvent), typeof(TrustSuspendedEventSchema));
        sink.RegisterSchema("TrustRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.TrustRevokedEvent), typeof(TrustRevokedEventSchema));

        sink.RegisterPayloadMapper("TrustAssessedEvent", e =>
        {
            var evt = (DomainEvents.TrustAssessedEvent)e;
            return new TrustAssessedEventSchema(
                evt.TrustId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.TrustCategory,
                evt.Descriptor.Score,
                evt.AssessedAt.Value);
        });
        sink.RegisterPayloadMapper("TrustActivatedEvent", e =>
        {
            var evt = (DomainEvents.TrustActivatedEvent)e;
            return new TrustActivatedEventSchema(evt.TrustId.Value);
        });
        sink.RegisterPayloadMapper("TrustSuspendedEvent", e =>
        {
            var evt = (DomainEvents.TrustSuspendedEvent)e;
            return new TrustSuspendedEventSchema(evt.TrustId.Value);
        });
        sink.RegisterPayloadMapper("TrustRevokedEvent", e =>
        {
            var evt = (DomainEvents.TrustRevokedEvent)e;
            return new TrustRevokedEventSchema(evt.TrustId.Value);
        });
    }
}
