namespace Whycespace.Platform.Api.Core.Contracts.Audit;

/// <summary>
/// Read-only chain anchoring trace.
/// Records that an execution was anchored to the WhyceChain.
/// No internal chain data exposed — only hash and timestamp.
/// </summary>
public sealed record ChainTraceView
{
    public required string Hash { get; init; }
    public required DateTimeOffset AnchoredAt { get; init; }
    public string? BlockReference { get; init; }
}
