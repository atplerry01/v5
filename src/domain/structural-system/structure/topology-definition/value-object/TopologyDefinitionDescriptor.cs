using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public readonly record struct TopologyDefinitionDescriptor
{
    public string DefinitionName { get; }
    public string DefinitionKind { get; }

    public TopologyDefinitionDescriptor(string definitionName, string definitionKind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(definitionName), "TopologyDefinitionDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(definitionKind), "TopologyDefinitionDescriptor kind must not be empty.");

        DefinitionName = definitionName;
        DefinitionKind = definitionKind;
    }
}
