using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed class ModerationSpecification : Specification<ModerationCaseStatus>
{
    public override bool IsSatisfiedBy(ModerationCaseStatus entity) =>
        entity != ModerationCaseStatus.Closed;

    public void EnsureMutable(ModerationCaseStatus status)
    {
        if (status == ModerationCaseStatus.Closed) throw ModerationErrors.CannotReopenClosed();
    }

    public void EnsureDecided(ModerationCaseStatus status)
    {
        if (status != ModerationCaseStatus.Decided && status != ModerationCaseStatus.Appealed)
            throw ModerationErrors.NotDecided();
    }

    public void EnsureValidDecision(ModerationDecision decision)
    {
        if (decision == ModerationDecision.Undecided) throw ModerationErrors.InvalidDecision();
    }
}
