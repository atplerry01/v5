namespace Whycespace.Engines.T2E.Trust.Identity.Identity;

public record IdentityResult(bool Success, string Message);
public sealed record IdentityDto(string IdentityId, string IdentityType, string DisplayName, string Status);
