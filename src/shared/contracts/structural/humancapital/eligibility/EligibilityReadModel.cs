namespace Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

public sealed record EligibilityReadModel
{
    public Guid EligibilityId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
