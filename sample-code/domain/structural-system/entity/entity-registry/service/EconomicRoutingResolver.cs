namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EconomicRoutingResolver
{
    public EconomicRoutingPath ResolveRouting(
        Guid source,
        Guid target,
        EntityExecutionPath executionPath)
    {
        return new EconomicRoutingPath(
            source,
            target,
            executionPath.Nodes);
    }
}
