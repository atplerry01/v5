namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Binding;

public sealed record CapitalBoundEventSchema(
    Guid AggregateId,
    Guid AccountId,
    Guid OwnerId,
    int OwnershipType,
    DateTimeOffset BoundAt);

public sealed record OwnershipTransferredEventSchema(
    Guid AggregateId,
    Guid PreviousOwnerId,
    Guid NewOwnerId,
    int NewOwnershipType,
    DateTimeOffset TransferredAt);

public sealed record BindingReleasedEventSchema(
    Guid AggregateId,
    Guid AccountId,
    DateTimeOffset ReleasedAt);
