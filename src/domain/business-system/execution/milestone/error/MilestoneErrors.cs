namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public static class MilestoneErrors
{
    public static MilestoneDomainException MissingId()
        => new("MilestoneId is required and must not be empty.");
    public static MilestoneDomainException MissingTargetId()
        => new("MilestoneTargetId is required and must not be empty.");
    public static MilestoneDomainException AlreadyReached()
        => new("Milestone has already been reached.");
    public static MilestoneDomainException AlreadyMissed()
        => new("Milestone has already been missed.");
    public static MilestoneDomainException InvalidStateTransition(MilestoneStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class MilestoneDomainException : Exception
{
    public MilestoneDomainException(string message) : base(message) { }
}
