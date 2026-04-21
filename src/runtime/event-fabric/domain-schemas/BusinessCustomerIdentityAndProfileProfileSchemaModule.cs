using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Profile;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/identity-and-profile/profile domain.
///
/// Owns the binding from Profile domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ProfileId, CustomerRef,
/// ProfileDisplayName, ProfileDescriptor) into the shared schema records
/// (Guid AggregateId + primitives) consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerIdentityAndProfileProfileSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProfileCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileCreatedEvent),
            typeof(ProfileCreatedEventSchema));

        sink.RegisterSchema(
            "ProfileRenamedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileRenamedEvent),
            typeof(ProfileRenamedEventSchema));

        sink.RegisterSchema(
            "ProfileDescriptorSetEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileDescriptorSetEvent),
            typeof(ProfileDescriptorSetEventSchema));

        sink.RegisterSchema(
            "ProfileDescriptorRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileDescriptorRemovedEvent),
            typeof(ProfileDescriptorRemovedEventSchema));

        sink.RegisterSchema(
            "ProfileActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileActivatedEvent),
            typeof(ProfileActivatedEventSchema));

        sink.RegisterSchema(
            "ProfileArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProfileArchivedEvent),
            typeof(ProfileArchivedEventSchema));

        sink.RegisterPayloadMapper("ProfileCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProfileCreatedEvent)e;
            return new ProfileCreatedEventSchema(
                evt.ProfileId.Value,
                evt.Customer.Value,
                evt.DisplayName.Value);
        });
        sink.RegisterPayloadMapper("ProfileRenamedEvent", e =>
        {
            var evt = (DomainEvents.ProfileRenamedEvent)e;
            return new ProfileRenamedEventSchema(evt.ProfileId.Value, evt.DisplayName.Value);
        });
        sink.RegisterPayloadMapper("ProfileDescriptorSetEvent", e =>
        {
            var evt = (DomainEvents.ProfileDescriptorSetEvent)e;
            return new ProfileDescriptorSetEventSchema(
                evt.ProfileId.Value,
                evt.Descriptor.Key,
                evt.Descriptor.Value);
        });
        sink.RegisterPayloadMapper("ProfileDescriptorRemovedEvent", e =>
        {
            var evt = (DomainEvents.ProfileDescriptorRemovedEvent)e;
            return new ProfileDescriptorRemovedEventSchema(evt.ProfileId.Value, evt.Key);
        });
        sink.RegisterPayloadMapper("ProfileActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProfileActivatedEvent)e;
            return new ProfileActivatedEventSchema(evt.ProfileId.Value);
        });
        sink.RegisterPayloadMapper("ProfileArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProfileArchivedEvent)e;
            return new ProfileArchivedEventSchema(evt.ProfileId.Value);
        });
    }
}
