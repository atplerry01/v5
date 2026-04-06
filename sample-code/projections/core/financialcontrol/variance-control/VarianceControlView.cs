namespace Whycespace.Projections.Core.Financialcontrol.VarianceControl;

public sealed record VarianceControlView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
