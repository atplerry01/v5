namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public sealed record EventMetadata
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public string SourceSystem { get; init; } = string.Empty;
    public string PartitionKey { get; init; } = string.Empty;
    public int RetryCount { get; init; }
    public bool IsReplay { get; init; }
}
