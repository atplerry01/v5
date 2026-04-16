using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public static class CallErrors
{
    public static DomainException InvalidActor() => new("Call actor reference must be non-empty.");
    public static DomainException InvalidTransition(CallStatus from, CallStatus to) =>
        new($"Illegal call state transition {from} -> {to}.");
    public static DomainException AlreadyEnded() => new("Call has already ended.");
    public static DomainException NotAnswerable() => new("Call cannot be answered from its current state.");
    public static DomainException ParticipantAlreadyLeft() => new("Participant has already left the call.");
    public static DomainException ParticipantNotInCall() => new("Participant is not in this call.");
    public static DomainInvariantViolationException InitiatorMissing() =>
        new("Invariant violated: call must have an initiator.");
}
