using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;

public sealed record InitiateSystemVerificationCommand(
    Guid VerificationId,
    string TargetSystem,
    DateTimeOffset InitiatedAt) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}

public sealed record PassSystemVerificationCommand(
    Guid VerificationId,
    DateTimeOffset PassedAt) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}

public sealed record FailSystemVerificationCommand(
    Guid VerificationId,
    string FailureReason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}
