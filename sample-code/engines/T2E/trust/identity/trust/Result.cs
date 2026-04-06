namespace Whycespace.Engines.T2E.Trust.Identity.Trust;

public record TrustResult(bool Success, string Message);
public sealed record TrustDto(string TrustProfileId, string IdentityId, decimal Score, string Level, string Status);
