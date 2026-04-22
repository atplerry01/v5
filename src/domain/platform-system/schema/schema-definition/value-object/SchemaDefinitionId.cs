using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public readonly record struct SchemaDefinitionId
{
    public Guid Value { get; }

    public SchemaDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SchemaDefinitionId cannot be empty.");
        Value = value;
    }
}
