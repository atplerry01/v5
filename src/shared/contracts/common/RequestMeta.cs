namespace Whyce.Shared.Contracts.Common;

/// <summary>
/// Optional metadata a caller may attach to a request envelope.
/// CorrelationId, if provided, is propagated into the runtime context.
/// </summary>
public sealed class RequestMeta
{
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
    public string? Timestamp { get; set; }
}
