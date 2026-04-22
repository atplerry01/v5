namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public readonly record struct SchemaStatus
{
    public static readonly SchemaStatus Draft = new("Draft");
    public static readonly SchemaStatus Published = new("Published");
    public static readonly SchemaStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private SchemaStatus(string value) => Value = value;
}
