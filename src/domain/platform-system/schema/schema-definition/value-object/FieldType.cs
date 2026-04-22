namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public readonly record struct FieldType
{
    public static readonly FieldType String = new("String");
    public static readonly FieldType Int = new("Int");
    public static readonly FieldType Long = new("Long");
    public static readonly FieldType Bool = new("Bool");
    public static readonly FieldType Float = new("Float");
    public static readonly FieldType Bytes = new("Bytes");
    public static readonly FieldType Nested = new("Nested");
    public static readonly FieldType Array = new("Array");
    public static readonly FieldType Map = new("Map");

    public string Value { get; }

    private FieldType(string value) => Value = value;
}
