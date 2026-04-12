namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public static class TopologyErrors
{
    public static InvalidOperationException MissingId()
        => new("TopologyId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("TopologyDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(TopologyStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
