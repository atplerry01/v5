using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Response;

public sealed class ResponseAggregate : AggregateRoot
{
    public static ResponseAggregate Create()
    {
        var aggregate = new ResponseAggregate();
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
