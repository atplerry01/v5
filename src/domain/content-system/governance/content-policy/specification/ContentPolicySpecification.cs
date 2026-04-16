using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public sealed class ContentPolicySpecification : Specification<ContentPolicyStatus>
{
    public override bool IsSatisfiedBy(ContentPolicyStatus entity) =>
        entity == ContentPolicyStatus.Draft || entity == ContentPolicyStatus.Published;

    public void EnsureMutable(ContentPolicyStatus status)
    {
        if (status == ContentPolicyStatus.Retired) throw ContentPolicyErrors.CannotMutateRetired();
    }

    public void EnsureAmendmentRevision(int current, int next)
    {
        if (next <= current) throw ContentPolicyErrors.AmendmentNotIncrementing();
    }
}
