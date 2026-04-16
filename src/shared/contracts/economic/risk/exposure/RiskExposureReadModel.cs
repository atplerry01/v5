namespace Whycespace.Shared.Contracts.Economic.Risk.Exposure;

public sealed record RiskExposureReadModel
{
    public Guid ExposureId { get; init; }
    public Guid SourceId { get; init; }
    public int ExposureType { get; init; }
    public decimal TotalExposure { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
