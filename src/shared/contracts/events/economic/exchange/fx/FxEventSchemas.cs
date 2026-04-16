namespace Whycespace.Shared.Contracts.Events.Economic.Exchange.Fx;

public sealed record FxPairRegisteredEventSchema(
    Guid AggregateId,
    string BaseCurrency,
    string QuoteCurrency);

public sealed record FxPairActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record FxPairDeactivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset DeactivatedAt);
