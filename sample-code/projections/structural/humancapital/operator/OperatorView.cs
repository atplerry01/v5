namespace Whycespace.Projections.Structural.Humancapital.Operator;

public sealed record OperatorView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
