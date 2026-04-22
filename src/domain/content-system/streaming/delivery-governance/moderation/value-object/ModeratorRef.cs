using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

/// Reference to the assigned moderator identity; bare Guid per domain.guard.md rule 13.
public readonly record struct ModeratorRef
{
    public Guid Value { get; }

    public ModeratorRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ModeratorRef cannot be empty.");
        Value = value;
    }
}
