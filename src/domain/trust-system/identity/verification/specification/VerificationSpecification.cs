using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class CanPassSpecification : Specification<VerificationAggregate>
{
    public override bool IsSatisfiedBy(VerificationAggregate aggregate) =>
        aggregate.Status == VerificationStatus.Initiated;
}

public sealed class CanFailSpecification : Specification<VerificationAggregate>
{
    public override bool IsSatisfiedBy(VerificationAggregate aggregate) =>
        aggregate.Status == VerificationStatus.Initiated;
}
