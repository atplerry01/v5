namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Consent;

public sealed record ConsentGrantedEventSchema(Guid AggregateId, Guid IdentityReference, string ConsentScope, string ConsentPurpose, DateTimeOffset GrantedAt);
public sealed record ConsentRevokedEventSchema(Guid AggregateId);
public sealed record ConsentExpiredEventSchema(Guid AggregateId);
