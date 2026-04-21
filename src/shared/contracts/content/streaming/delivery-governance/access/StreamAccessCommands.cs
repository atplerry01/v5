using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;

public sealed record GrantStreamAccessCommand(
    Guid AccessId,
    Guid StreamId,
    string Mode,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    string Token,
    DateTimeOffset GrantedAt) : IHasAggregateId
{
    public Guid AggregateId => AccessId;
}

public sealed record RestrictStreamAccessCommand(
    Guid AccessId,
    string Reason,
    DateTimeOffset RestrictedAt) : IHasAggregateId
{
    public Guid AggregateId => AccessId;
}

public sealed record UnrestrictStreamAccessCommand(
    Guid AccessId,
    DateTimeOffset UnrestrictedAt) : IHasAggregateId
{
    public Guid AggregateId => AccessId;
}

public sealed record RevokeStreamAccessCommand(
    Guid AccessId,
    string Reason,
    DateTimeOffset RevokedAt) : IHasAggregateId
{
    public Guid AggregateId => AccessId;
}

public sealed record ExpireStreamAccessCommand(
    Guid AccessId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => AccessId;
}
