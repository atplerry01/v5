namespace Whycespace.Projections.Business.Execution.Setup;

public sealed record SetupView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
