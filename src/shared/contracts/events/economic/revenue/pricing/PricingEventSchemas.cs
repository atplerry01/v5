namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Pricing;

public sealed record PriceDefinedEventSchema(
    Guid AggregateId,
    Guid ContractId,
    string Model,
    decimal Price,
    string Currency,
    DateTimeOffset DefinedAt);

public sealed record PriceAdjustedEventSchema(
    Guid AggregateId,
    decimal PreviousPrice,
    decimal NewPrice,
    string Reason,
    DateTimeOffset AdjustedAt);
