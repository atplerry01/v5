using Whycespace.Shared.Contracts.Events.Trust.Identity.Profile;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Profile;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityProfileSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ProfileCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProfileCreatedEvent), typeof(ProfileCreatedEventSchema));
        sink.RegisterSchema("ProfileActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProfileActivatedEvent), typeof(ProfileActivatedEventSchema));
        sink.RegisterSchema("ProfileDeactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProfileDeactivatedEvent), typeof(ProfileDeactivatedEventSchema));

        sink.RegisterPayloadMapper("ProfileCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProfileCreatedEvent)e;
            return new ProfileCreatedEventSchema(
                evt.ProfileId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.DisplayName,
                evt.Descriptor.ProfileType,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("ProfileActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProfileActivatedEvent)e;
            return new ProfileActivatedEventSchema(evt.ProfileId.Value);
        });
        sink.RegisterPayloadMapper("ProfileDeactivatedEvent", e =>
        {
            var evt = (DomainEvents.ProfileDeactivatedEvent)e;
            return new ProfileDeactivatedEventSchema(evt.ProfileId.Value);
        });
    }
}
