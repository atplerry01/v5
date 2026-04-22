namespace Whycespace.Shared.Contracts.Events.Trust.Access.Permission;

public sealed record PermissionDefinedEventSchema(Guid AggregateId, string PermissionName, string ResourceType);
public sealed record PermissionActivatedEventSchema(Guid AggregateId);
public sealed record PermissionDeprecatedEventSchema(Guid AggregateId);
