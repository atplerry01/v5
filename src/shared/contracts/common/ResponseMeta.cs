namespace Whyce.Shared.Contracts.Common;

/// <summary>
/// Metadata included in every API response. Never null — factory methods
/// guarantee population with at minimum a correlationId and timestamp.
/// </summary>
public sealed class ResponseMeta
{
    public string CorrelationId { get; set; } = default!;
    public string Timestamp { get; set; } = default!;
    public string? RequestId { get; set; }
    public string? Version { get; set; }
}
