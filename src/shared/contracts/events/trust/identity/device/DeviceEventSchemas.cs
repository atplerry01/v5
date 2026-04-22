namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Device;

public sealed record DeviceRegisteredEventSchema(Guid AggregateId, Guid IdentityReference, string DeviceName, string DeviceType, DateTimeOffset RegisteredAt);
public sealed record DeviceActivatedEventSchema(Guid AggregateId);
public sealed record DeviceSuspendedEventSchema(Guid AggregateId);
public sealed record DeviceDeregisteredEventSchema(Guid AggregateId);
