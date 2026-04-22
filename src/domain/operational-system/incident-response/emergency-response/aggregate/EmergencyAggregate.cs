using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OperationalSystem.IncidentResponse.EmergencyResponse;

public sealed class EmergencyAggregate : AggregateRoot
{
    public static EmergencyAggregate Create()
    {
        var aggregate = new EmergencyAggregate();
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
