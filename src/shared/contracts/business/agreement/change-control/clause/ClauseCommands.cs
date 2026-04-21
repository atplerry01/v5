using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;

public sealed record CreateClauseCommand(Guid ClauseId, string ClauseType) : IHasAggregateId
{
    public Guid AggregateId => ClauseId;
}

public sealed record ActivateClauseCommand(Guid ClauseId) : IHasAggregateId
{
    public Guid AggregateId => ClauseId;
}

public sealed record SupersedeClauseCommand(Guid ClauseId) : IHasAggregateId
{
    public Guid AggregateId => ClauseId;
}
