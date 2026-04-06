namespace Whycespace.Platform.Api.Business.Agreement.Signature;

public sealed record SignatureRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SignatureResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
