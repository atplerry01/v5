namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.Authorization;

public sealed record AuthorizationGrantedEventSchema(
    Guid AggregateId,
    string SubjectId,
    IReadOnlyList<string> RoleIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo);

public sealed record AuthorizationRevokedEventSchema(
    Guid AggregateId);
