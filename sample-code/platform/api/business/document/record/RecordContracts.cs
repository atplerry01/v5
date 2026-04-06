namespace Whycespace.Platform.Api.Business.Document.Record;

public sealed record RecordRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RecordResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
