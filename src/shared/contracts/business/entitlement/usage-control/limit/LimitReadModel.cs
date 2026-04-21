namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;

public sealed record LimitReadModel
{
    public Guid LimitId { get; init; }
    public Guid SubjectId { get; init; }
    public int ThresholdValue { get; init; }
    public int ObservedValue { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
