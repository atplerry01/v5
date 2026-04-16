using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;

public sealed record ApplyRestrictionCommand(
    Guid RestrictionId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset AppliedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}

public sealed record UpdateRestrictionCommand(
    Guid RestrictionId,
    string NewScope,
    string NewReason,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}

public sealed record RemoveRestrictionCommand(
    Guid RestrictionId,
    string RemovalReason,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}
