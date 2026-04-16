using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;

public sealed record IssueSanctionCommand(
    Guid SanctionId,
    Guid SubjectId,
    string Type,
    string Scope,
    string Reason,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => SanctionId;
}

public sealed record ActivateSanctionCommand(
    Guid SanctionId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => SanctionId;
}

public sealed record RevokeSanctionCommand(
    Guid SanctionId,
    string RevocationReason,
    DateTimeOffset RevokedAt) : IHasAggregateId
{
    public Guid AggregateId => SanctionId;
}

public sealed record ExpireSanctionCommand(
    Guid SanctionId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => SanctionId;
}
