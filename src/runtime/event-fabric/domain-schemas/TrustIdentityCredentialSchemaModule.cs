using Whycespace.Shared.Contracts.Events.Trust.Identity.Credential;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Credential;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityCredentialSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("CredentialIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.CredentialIssuedEvent), typeof(CredentialIssuedEventSchema));
        sink.RegisterSchema("CredentialActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.CredentialActivatedEvent), typeof(CredentialActivatedEventSchema));
        sink.RegisterSchema("CredentialRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.CredentialRevokedEvent), typeof(CredentialRevokedEventSchema));

        sink.RegisterPayloadMapper("CredentialIssuedEvent", e =>
        {
            var evt = (DomainEvents.CredentialIssuedEvent)e;
            return new CredentialIssuedEventSchema(
                evt.CredentialId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.CredentialType);
        });
        sink.RegisterPayloadMapper("CredentialActivatedEvent", e =>
        {
            var evt = (DomainEvents.CredentialActivatedEvent)e;
            return new CredentialActivatedEventSchema(evt.CredentialId.Value);
        });
        sink.RegisterPayloadMapper("CredentialRevokedEvent", e =>
        {
            var evt = (DomainEvents.CredentialRevokedEvent)e;
            return new CredentialRevokedEventSchema(evt.CredentialId.Value);
        });
    }
}
