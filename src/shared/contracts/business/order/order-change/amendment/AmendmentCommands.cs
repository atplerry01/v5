using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;

public sealed record RequestAmendmentCommand(
    Guid AmendmentId,
    Guid OrderId,
    string Reason,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record AcceptAmendmentCommand(
    Guid AmendmentId,
    DateTimeOffset AcceptedAt) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record ApplyAmendmentCommand(
    Guid AmendmentId,
    DateTimeOffset AppliedAt) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record RejectAmendmentCommand(
    Guid AmendmentId,
    DateTimeOffset RejectedAt) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record CancelAmendmentCommand(
    Guid AmendmentId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}
