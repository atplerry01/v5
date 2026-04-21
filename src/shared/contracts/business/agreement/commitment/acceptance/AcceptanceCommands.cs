using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;

public sealed record CreateAcceptanceCommand(Guid AcceptanceId) : IHasAggregateId
{
    public Guid AggregateId => AcceptanceId;
}

public sealed record AcceptAcceptanceCommand(Guid AcceptanceId) : IHasAggregateId
{
    public Guid AggregateId => AcceptanceId;
}

public sealed record RejectAcceptanceCommand(Guid AcceptanceId) : IHasAggregateId
{
    public Guid AggregateId => AcceptanceId;
}
