namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public readonly record struct SchemaChangeType
{
    public static readonly SchemaChangeType FieldAdded = new("FieldAdded");
    public static readonly SchemaChangeType FieldRemoved = new("FieldRemoved");
    public static readonly SchemaChangeType FieldTypeChanged = new("FieldTypeChanged");
    public static readonly SchemaChangeType FieldRequiredChanged = new("FieldRequiredChanged");
    public static readonly SchemaChangeType FieldRenamed = new("FieldRenamed");

    public string Value { get; }

    private SchemaChangeType(string value) => Value = value;
}
