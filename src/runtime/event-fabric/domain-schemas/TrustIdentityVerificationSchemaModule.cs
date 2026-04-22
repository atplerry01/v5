using Whycespace.Shared.Contracts.Events.Trust.Identity.Verification;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Verification;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityVerificationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("VerificationInitiatedEvent", EventVersion.Default,
            typeof(DomainEvents.VerificationInitiatedEvent), typeof(VerificationInitiatedEventSchema));
        sink.RegisterSchema("VerificationPassedEvent", EventVersion.Default,
            typeof(DomainEvents.VerificationPassedEvent), typeof(VerificationPassedEventSchema));
        sink.RegisterSchema("VerificationFailedEvent", EventVersion.Default,
            typeof(DomainEvents.VerificationFailedEvent), typeof(VerificationFailedEventSchema));

        sink.RegisterPayloadMapper("VerificationInitiatedEvent", e =>
        {
            var evt = (DomainEvents.VerificationInitiatedEvent)e;
            return new VerificationInitiatedEventSchema(
                evt.VerificationId.Value,
                evt.Subject.IdentityReference,
                evt.Subject.ClaimType);
        });
        sink.RegisterPayloadMapper("VerificationPassedEvent", e =>
        {
            var evt = (DomainEvents.VerificationPassedEvent)e;
            return new VerificationPassedEventSchema(evt.VerificationId.Value);
        });
        sink.RegisterPayloadMapper("VerificationFailedEvent", e =>
        {
            var evt = (DomainEvents.VerificationFailedEvent)e;
            return new VerificationFailedEventSchema(evt.VerificationId.Value);
        });
    }
}
