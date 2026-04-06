namespace Whycespace.Projections.Core.Financialcontrol.ReserveControl;

public sealed record ReserveControlView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
