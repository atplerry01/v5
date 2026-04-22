namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public readonly record struct SchemaCompatibilityMode
{
    public static readonly SchemaCompatibilityMode Backward = new("Backward");
    public static readonly SchemaCompatibilityMode Forward = new("Forward");
    public static readonly SchemaCompatibilityMode Full = new("Full");
    public static readonly SchemaCompatibilityMode None = new("None");

    public string Value { get; }

    private SchemaCompatibilityMode(string value) => Value = value;
}
