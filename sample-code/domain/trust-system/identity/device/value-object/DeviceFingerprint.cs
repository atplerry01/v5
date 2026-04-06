namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceFingerprint
{
    public string Value { get; }

    public DeviceFingerprint(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DeviceFingerprint cannot be empty.", nameof(value));
        Value = value;
    }

    public override string ToString() => "***FINGERPRINT***";
}
