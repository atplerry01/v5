using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.IntelligenceSystem.Relationship.Graph;

public sealed class GraphAggregate : AggregateRoot
{
    public static GraphAggregate Create()
    {
        var aggregate = new GraphAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
