using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public static class PresenceErrors
{
    public static DomainException InvalidActor() => new("Presence actor reference must be non-empty.");
    public static DomainException InvalidTransition(PresenceStatus from, PresenceStatus to) =>
        new($"Illegal presence transition {from} -> {to}.");
    public static DomainException AlreadyExpired() => new("Presence has already expired.");
    public static DomainException NotRegistered() => new("Presence has not been registered.");
    public static DomainInvariantViolationException ActorMissing() =>
        new("Invariant violated: presence must have an actor reference.");
}
