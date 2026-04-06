namespace Whycespace.Projections.Business.Logistic.Shipment;

public sealed record ShipmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
