namespace Whycespace.Projections.Business.Execution.Charge;

public sealed record ChargeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
