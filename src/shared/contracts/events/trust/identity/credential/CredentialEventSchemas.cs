namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Credential;

public sealed record CredentialIssuedEventSchema(Guid AggregateId, Guid IdentityReference, string CredentialType);
public sealed record CredentialActivatedEventSchema(Guid AggregateId);
public sealed record CredentialRevokedEventSchema(Guid AggregateId);
