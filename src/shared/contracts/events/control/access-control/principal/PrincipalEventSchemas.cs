namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.Principal;

public sealed record PrincipalRegisteredEventSchema(
    Guid AggregateId,
    string Name,
    string Kind,
    string IdentityId);

public sealed record PrincipalRoleAssignedEventSchema(
    Guid AggregateId,
    string RoleId);

public sealed record PrincipalDeactivatedEventSchema(
    Guid AggregateId);
