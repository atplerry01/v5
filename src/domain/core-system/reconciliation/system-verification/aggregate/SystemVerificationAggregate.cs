using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public sealed class SystemVerificationAggregate : AggregateRoot
{
    public static SystemVerificationAggregate Create()
    {
        var aggregate = new SystemVerificationAggregate();
        if (aggregate.Version >= 0)
            throw SystemVerificationErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
