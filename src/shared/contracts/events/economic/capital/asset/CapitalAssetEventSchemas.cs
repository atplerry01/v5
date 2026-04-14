namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Asset;

public sealed record AssetCreatedEventSchema(
    Guid AggregateId,
    Guid OwnerId,
    decimal InitialValue,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record AssetValuedEventSchema(
    Guid AggregateId,
    decimal PreviousValue,
    decimal NewValue,
    string Currency,
    DateTimeOffset ValuedAt);

public sealed record AssetDisposedEventSchema(
    Guid AggregateId,
    decimal FinalValue,
    DateTimeOffset DisposedAt);
