namespace Whycespace.Platform.Api.Business.Document.SignatureRecord;

public sealed record SignatureRecordRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SignatureRecordResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
