using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public sealed record FieldDescriptor
{
    public string FieldName { get; }
    public FieldType FieldType { get; }
    public bool Required { get; }
    public string? DefaultValue { get; }
    public string? Description { get; }

    public FieldDescriptor(string fieldName, FieldType fieldType, bool required, string? defaultValue, string? description)
    {
        Guard.Against(string.IsNullOrWhiteSpace(fieldName), "FieldDescriptor requires a non-empty FieldName.");
        FieldName = fieldName;
        FieldType = fieldType;
        Required = required;
        DefaultValue = defaultValue;
        Description = description;
    }
}
