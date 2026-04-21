using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;

public sealed record CreateObligationCommand(Guid ObligationId) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}

public sealed record FulfillObligationCommand(Guid ObligationId) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}

public sealed record BreachObligationCommand(Guid ObligationId) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}
