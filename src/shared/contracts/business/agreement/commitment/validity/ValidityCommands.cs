using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;

public sealed record CreateValidityCommand(Guid ValidityId) : IHasAggregateId
{
    public Guid AggregateId => ValidityId;
}

public sealed record ExpireValidityCommand(Guid ValidityId) : IHasAggregateId
{
    public Guid AggregateId => ValidityId;
}

public sealed record InvalidateValidityCommand(Guid ValidityId) : IHasAggregateId
{
    public Guid AggregateId => ValidityId;
}
