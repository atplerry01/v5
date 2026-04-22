namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.Permission;

public sealed record PermissionDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string ResourceScope,
    string Actions);

public sealed record PermissionDeprecatedEventSchema(
    Guid AggregateId);
