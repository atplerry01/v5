namespace Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

public sealed record WorkforceReadModel
{
    public Guid WorkforceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
