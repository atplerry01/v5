using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public readonly record struct SchemaName
{
    public string Value { get; }

    public SchemaName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SchemaName cannot be empty.");
        Value = value;
    }
}
