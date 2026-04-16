using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed class PresenceSpecification : Specification<PresenceStatus>
{
    private static readonly IReadOnlySet<PresenceStatus> Active = new HashSet<PresenceStatus>
    {
        PresenceStatus.Online, PresenceStatus.Away, PresenceStatus.Busy
    };

    public override bool IsSatisfiedBy(PresenceStatus entity) => Active.Contains(entity);

    public void EnsureCanChangeTo(PresenceStatus from, PresenceStatus to)
    {
        if (from == PresenceStatus.Expired)
            throw PresenceErrors.InvalidTransition(from, to);
        if (to == PresenceStatus.Expired && from == PresenceStatus.Expired)
            throw PresenceErrors.AlreadyExpired();
    }
}
