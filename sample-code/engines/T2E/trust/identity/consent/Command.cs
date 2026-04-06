namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public record ConsentCommand(string Action, string EntityId, object Payload);
public sealed record GrantConsentCommand(string IdentityId, string ConsentType, string Scope, string? ExpiryDate) : ConsentCommand("Grant", IdentityId, null!);
public sealed record RevokeConsentCommand(string ConsentId, string Reason) : ConsentCommand("Revoke", ConsentId, null!);
public sealed record ExpireConsentCommand(string ConsentId) : ConsentCommand("Expire", ConsentId, null!);
