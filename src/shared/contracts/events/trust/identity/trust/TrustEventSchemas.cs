namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Trust;

public sealed record TrustAssessedEventSchema(Guid AggregateId, Guid IdentityReference, string TrustCategory, decimal Score, DateTimeOffset AssessedAt);
public sealed record TrustActivatedEventSchema(Guid AggregateId);
public sealed record TrustSuspendedEventSchema(Guid AggregateId);
public sealed record TrustRevokedEventSchema(Guid AggregateId);
