namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceType(string Value)
{
    public static readonly DeviceType Mobile = new("Mobile");
    public static readonly DeviceType Desktop = new("Desktop");
    public static readonly DeviceType Tablet = new("Tablet");
    public static readonly DeviceType IoT = new("IoT");
    public static readonly DeviceType Browser = new("Browser");

    public override string ToString() => Value;
}
