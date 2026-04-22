namespace Whycespace.Shared.Contracts.Events.Trust.Identity.ServiceIdentity;

public sealed record ServiceIdentityRegisteredEventSchema(Guid AggregateId, Guid OwnerReference, string ServiceName, string ServiceType, DateTimeOffset RegisteredAt);
public sealed record ServiceIdentitySuspendedEventSchema(Guid AggregateId);
public sealed record ServiceIdentityDecommissionedEventSchema(Guid AggregateId);
