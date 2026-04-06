namespace Whycespace.Engines.T2E.Trust.Identity.Device;

public record DeviceResult(bool Success, string Message);
public sealed record DeviceDto(string DeviceId, string IdentityId, string DeviceType, string Status);
