using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Lock;

public sealed record LockSystemCommand(
    Guid LockId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset LockedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}

public sealed record UnlockSystemCommand(
    Guid LockId,
    string UnlockReason,
    DateTimeOffset UnlockedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}
