namespace Whycespace.Projections.Business.Execution.Activation;

public sealed record ActivationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
