namespace Whycespace.Shared.Contracts.Events.Trust.Access.Role;

public sealed record RoleDefinedEventSchema(Guid AggregateId, string RoleName, string RoleScope, DateTimeOffset DefinedAt);
public sealed record RoleActivatedEventSchema(Guid AggregateId);
public sealed record RoleDeprecatedEventSchema(Guid AggregateId);
