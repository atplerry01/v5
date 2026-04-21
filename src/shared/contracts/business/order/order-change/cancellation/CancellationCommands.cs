using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;

public sealed record RequestCancellationCommand(
    Guid CancellationId,
    Guid OrderId,
    string Reason,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => CancellationId;
}

public sealed record ConfirmCancellationCommand(
    Guid CancellationId,
    DateTimeOffset ConfirmedAt) : IHasAggregateId
{
    public Guid AggregateId => CancellationId;
}

public sealed record RejectCancellationCommand(
    Guid CancellationId,
    DateTimeOffset RejectedAt) : IHasAggregateId
{
    public Guid AggregateId => CancellationId;
}
