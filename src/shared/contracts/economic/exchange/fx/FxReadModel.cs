namespace Whycespace.Shared.Contracts.Economic.Exchange.Fx;

public sealed record FxReadModel
{
    public Guid FxId { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string QuoteCurrency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
