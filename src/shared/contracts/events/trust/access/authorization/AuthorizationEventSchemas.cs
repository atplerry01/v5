namespace Whycespace.Shared.Contracts.Events.Trust.Access.Authorization;

public sealed record AuthorizationGrantedEventSchema(Guid AggregateId, Guid PrincipalReference, string ResourceReference);
public sealed record AuthorizationDeniedEventSchema(Guid AggregateId, Guid PrincipalReference, string ResourceReference);
public sealed record AuthorizationRevokedEventSchema(Guid AggregateId);
