namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TopologyDefinitionStatus status)
        => status == TopologyDefinitionStatus.Draft;
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(TopologyDefinitionStatus status)
        => status == TopologyDefinitionStatus.Active;
}

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(TopologyDefinitionStatus status)
        => status == TopologyDefinitionStatus.Suspended;
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(TopologyDefinitionStatus status)
        => status == TopologyDefinitionStatus.Active || status == TopologyDefinitionStatus.Suspended;
}
