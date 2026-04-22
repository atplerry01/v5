using Whycespace.Shared.Contracts.Events.Trust.Identity.Device;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Device;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityDeviceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DeviceRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.DeviceRegisteredEvent), typeof(DeviceRegisteredEventSchema));
        sink.RegisterSchema("DeviceActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.DeviceActivatedEvent), typeof(DeviceActivatedEventSchema));
        sink.RegisterSchema("DeviceSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.DeviceSuspendedEvent), typeof(DeviceSuspendedEventSchema));
        sink.RegisterSchema("DeviceDeregisteredEvent", EventVersion.Default,
            typeof(DomainEvents.DeviceDeregisteredEvent), typeof(DeviceDeregisteredEventSchema));

        sink.RegisterPayloadMapper("DeviceRegisteredEvent", e =>
        {
            var evt = (DomainEvents.DeviceRegisteredEvent)e;
            return new DeviceRegisteredEventSchema(
                evt.DeviceId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.DeviceName,
                evt.Descriptor.DeviceType,
                evt.RegisteredAt.Value);
        });
        sink.RegisterPayloadMapper("DeviceActivatedEvent", e =>
        {
            var evt = (DomainEvents.DeviceActivatedEvent)e;
            return new DeviceActivatedEventSchema(evt.DeviceId.Value);
        });
        sink.RegisterPayloadMapper("DeviceSuspendedEvent", e =>
        {
            var evt = (DomainEvents.DeviceSuspendedEvent)e;
            return new DeviceSuspendedEventSchema(evt.DeviceId.Value);
        });
        sink.RegisterPayloadMapper("DeviceDeregisteredEvent", e =>
        {
            var evt = (DomainEvents.DeviceDeregisteredEvent)e;
            return new DeviceDeregisteredEventSchema(evt.DeviceId.Value);
        });
    }
}
