namespace Whycespace.Shared.Contracts.Economic.Capital.Binding;

public sealed record BindCapitalCommand(
    Guid BindingId,
    Guid AccountId,
    Guid OwnerId,
    int OwnershipType,
    DateTimeOffset BoundAt);

public sealed record TransferBindingOwnershipCommand(
    Guid BindingId,
    Guid NewOwnerId,
    int NewOwnershipType,
    DateTimeOffset TransferredAt);

public sealed record ReleaseBindingCommand(
    Guid BindingId,
    DateTimeOffset ReleasedAt);
