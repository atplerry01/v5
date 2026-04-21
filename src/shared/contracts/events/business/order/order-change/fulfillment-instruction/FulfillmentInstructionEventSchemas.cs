namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionCreatedEventSchema(
    Guid AggregateId,
    Guid OrderId,
    Guid? LineItemId,
    string Directive);

public sealed record FulfillmentInstructionIssuedEventSchema(Guid AggregateId, DateTimeOffset IssuedAt);

public sealed record FulfillmentInstructionCompletedEventSchema(Guid AggregateId, DateTimeOffset CompletedAt);

public sealed record FulfillmentInstructionRevokedEventSchema(Guid AggregateId, DateTimeOffset RevokedAt);
