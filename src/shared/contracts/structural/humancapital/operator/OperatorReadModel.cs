namespace Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

public sealed record OperatorReadModel
{
    public Guid OperatorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
