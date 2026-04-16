using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Binding;

public sealed record BindCapitalCommand(
    Guid BindingId,
    Guid AccountId,
    Guid OwnerId,
    int OwnershipType,
    DateTimeOffset BoundAt) : IHasAggregateId
{
    public Guid AggregateId => BindingId;
}

public sealed record TransferBindingOwnershipCommand(
    Guid BindingId,
    Guid NewOwnerId,
    int NewOwnershipType,
    DateTimeOffset TransferredAt) : IHasAggregateId
{
    public Guid AggregateId => BindingId;
}

public sealed record ReleaseBindingCommand(
    Guid BindingId,
    DateTimeOffset ReleasedAt) : IHasAggregateId
{
    public Guid AggregateId => BindingId;
}
