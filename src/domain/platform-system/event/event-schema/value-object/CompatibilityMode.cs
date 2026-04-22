namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public readonly record struct CompatibilityMode
{
    public static readonly CompatibilityMode Backward = new("Backward");
    public static readonly CompatibilityMode Forward = new("Forward");
    public static readonly CompatibilityMode Full = new("Full");
    public static readonly CompatibilityMode None = new("None");

    public string Value { get; }

    private CompatibilityMode(string value) => Value = value;
}
