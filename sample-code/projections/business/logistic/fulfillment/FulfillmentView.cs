namespace Whycespace.Projections.Business.Logistic.Fulfillment;

public sealed record FulfillmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
