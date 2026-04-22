namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Verification;

public sealed record VerificationInitiatedEventSchema(Guid AggregateId, Guid IdentityReference, string ClaimType);
public sealed record VerificationPassedEventSchema(Guid AggregateId);
public sealed record VerificationFailedEventSchema(Guid AggregateId);
