namespace Whycespace.Engines.T2E.Trust.Identity.Device;

public record DeviceCommand(string Action, string EntityId, object Payload);
public sealed record RegisterDeviceCommand(string IdentityId, string DeviceType, string Fingerprint) : DeviceCommand("Register", IdentityId, null!);
public sealed record VerifyDeviceCommand(string DeviceId) : DeviceCommand("Verify", DeviceId, null!);
public sealed record BlockDeviceCommand(string DeviceId, string Reason) : DeviceCommand("Block", DeviceId, null!);
public sealed record UnblockDeviceCommand(string DeviceId) : DeviceCommand("Unblock", DeviceId, null!);
public sealed record DeregisterDeviceCommand(string DeviceId, string Reason) : DeviceCommand("Deregister", DeviceId, null!);
