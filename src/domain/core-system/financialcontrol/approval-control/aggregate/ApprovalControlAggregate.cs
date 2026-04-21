using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ApprovalControl;

public sealed class ApprovalControlAggregate : AggregateRoot
{
    public static ApprovalControlAggregate Create()
    {
        var aggregate = new ApprovalControlAggregate();
        if (aggregate.Version >= 0)
            throw ApprovalControlErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
