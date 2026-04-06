namespace Whycespace.Platform.Api.Business.Document.Evidence;

public sealed record EvidenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EvidenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
