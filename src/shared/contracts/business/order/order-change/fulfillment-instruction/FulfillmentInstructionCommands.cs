using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;

public sealed record CreateFulfillmentInstructionCommand(
    Guid FulfillmentInstructionId,
    Guid OrderId,
    string Directive,
    Guid? LineItemId) : IHasAggregateId
{
    public Guid AggregateId => FulfillmentInstructionId;
}

public sealed record IssueFulfillmentInstructionCommand(
    Guid FulfillmentInstructionId,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => FulfillmentInstructionId;
}

public sealed record CompleteFulfillmentInstructionCommand(
    Guid FulfillmentInstructionId,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => FulfillmentInstructionId;
}

public sealed record RevokeFulfillmentInstructionCommand(
    Guid FulfillmentInstructionId,
    DateTimeOffset RevokedAt) : IHasAggregateId
{
    public Guid AggregateId => FulfillmentInstructionId;
}
