using Whycespace.Shared.Contracts.Events.Trust.Identity.Federation;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Federation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityFederationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("FederationEstablishedEvent", EventVersion.Default,
            typeof(DomainEvents.FederationEstablishedEvent), typeof(FederationEstablishedEventSchema));
        sink.RegisterSchema("FederationSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.FederationSuspendedEvent), typeof(FederationSuspendedEventSchema));
        sink.RegisterSchema("FederationTerminatedEvent", EventVersion.Default,
            typeof(DomainEvents.FederationTerminatedEvent), typeof(FederationTerminatedEventSchema));

        sink.RegisterPayloadMapper("FederationEstablishedEvent", e =>
        {
            var evt = (DomainEvents.FederationEstablishedEvent)e;
            return new FederationEstablishedEventSchema(
                evt.FederationId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.FederatedProvider,
                evt.Descriptor.FederationType,
                evt.EstablishedAt.Value);
        });
        sink.RegisterPayloadMapper("FederationSuspendedEvent", e =>
        {
            var evt = (DomainEvents.FederationSuspendedEvent)e;
            return new FederationSuspendedEventSchema(evt.FederationId.Value);
        });
        sink.RegisterPayloadMapper("FederationTerminatedEvent", e =>
        {
            var evt = (DomainEvents.FederationTerminatedEvent)e;
            return new FederationTerminatedEventSchema(evt.FederationId.Value);
        });
    }
}
