namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public static class HandoffErrors
{
    public static HandoffDomainException MissingId()
        => new("HandoffId is required and must not be empty.");

    public static HandoffDomainException MissingSourceActor()
        => new("Source ActorReference is required and must not be empty.");

    public static HandoffDomainException MissingTargetActor()
        => new("Target ActorReference is required and must not be empty.");

    public static HandoffDomainException ActorsCannotBeEqual(ActorReference actor)
        => new($"Source and target actors must reference different parties. Both reference '{actor.Value}'.");

    public static HandoffDomainException MissingTransferReference()
        => new("TransferReference is required and must not be empty.");

    public static HandoffDomainException AlreadyTransferred()
        => new("Handoff has already been transferred and cannot be transferred again.");
}

public sealed class HandoffDomainException : Exception
{
    public HandoffDomainException(string message) : base(message) { }
}
