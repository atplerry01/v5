using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public sealed class ComplianceSpecification : Specification<ComplianceCheckStatus>
{
    public override bool IsSatisfiedBy(ComplianceCheckStatus entity) => entity == ComplianceCheckStatus.Initiated;

    public void EnsureInitiated(ComplianceCheckStatus status)
    {
        if (status != ComplianceCheckStatus.Initiated)
            throw ComplianceErrors.CannotTransitionFromTerminal(status);
    }
}
