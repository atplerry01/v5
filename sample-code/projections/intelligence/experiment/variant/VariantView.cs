namespace Whycespace.Projections.Intelligence.Experiment.Variant;

public sealed record VariantView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
