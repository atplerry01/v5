namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public readonly record struct CompatibilityVerdict
{
    public static readonly CompatibilityVerdict Compatible = new("Compatible");
    public static readonly CompatibilityVerdict ConditionallyCompatible = new("ConditionallyCompatible");
    public static readonly CompatibilityVerdict Incompatible = new("Incompatible");

    public string Value { get; }

    private CompatibilityVerdict(string value) => Value = value;
}
