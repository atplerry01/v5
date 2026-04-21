using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public readonly record struct TopologyDefinitionId
{
    public Guid Value { get; }

    public TopologyDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TopologyDefinitionId cannot be empty.");
        Value = value;
    }
}
