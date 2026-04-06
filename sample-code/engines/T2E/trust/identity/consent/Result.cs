namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public record ConsentResult(bool Success, string Message);
public sealed record ConsentDto(string ConsentId, string IdentityId, string ConsentType, string Scope, string Status);
