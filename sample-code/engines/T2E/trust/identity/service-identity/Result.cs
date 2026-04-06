namespace Whycespace.Engines.T2E.Trust.Identity.ServiceIdentity;

public record ServiceIdentityResult(bool Success, string Message);
public sealed record ServiceIdentityDto(string ServiceIdentityId, string ServiceName, string ServiceType, string Status);
