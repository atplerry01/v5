namespace Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessGrantedEventSchema(
    Guid AggregateId,
    Guid StreamId,
    string Mode,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    string Token,
    DateTimeOffset GrantedAt);

public sealed record StreamAccessRestrictedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset RestrictedAt);

public sealed record StreamAccessUnrestrictedEventSchema(
    Guid AggregateId,
    DateTimeOffset UnrestrictedAt);

public sealed record StreamAccessRevokedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset RevokedAt);

public sealed record StreamAccessExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);
