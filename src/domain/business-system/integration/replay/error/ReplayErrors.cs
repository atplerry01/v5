namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public static class ReplayErrors
{
    public static ReplayDomainException MissingId()
        => new("ReplayId is required and must not be empty.");

    public static ReplayDomainException MissingPolicyId()
        => new("ReplayPolicyId is required and must not be empty.");

    public static ReplayDomainException InvalidStateTransition(ReplayStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ReplayDomainException AlreadyActive(ReplayId id)
        => new($"Replay '{id.Value}' is already active.");

    public static ReplayDomainException AlreadyDisabled(ReplayId id)
        => new($"Replay '{id.Value}' is already disabled.");
}

public sealed class ReplayDomainException : Exception
{
    public ReplayDomainException(string message) : base(message) { }
}
