namespace Whycespace.Shared.Contracts.Events.Trust.Access.Grant;

public sealed record GrantIssuedEventSchema(Guid AggregateId, Guid PrincipalReference, string GrantScope, string GrantType, DateTimeOffset IssuedAt);
public sealed record GrantActivatedEventSchema(Guid AggregateId);
public sealed record GrantRevokedEventSchema(Guid AggregateId);
public sealed record GrantExpiredEventSchema(Guid AggregateId);
