namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceStatus(string Value)
{
    public static readonly DeviceStatus Registered = new("Registered");
    public static readonly DeviceStatus Verified = new("Verified");
    public static readonly DeviceStatus Blocked = new("Blocked");
    public static readonly DeviceStatus Deregistered = new("Deregistered");

    public override string ToString() => Value;
}
