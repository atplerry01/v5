namespace Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;

public sealed record FulfillmentInstructionReadModel
{
    public Guid FulfillmentInstructionId { get; init; }
    public Guid OrderId { get; init; }
    public Guid? LineItemId { get; init; }
    public string Directive { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
