namespace Whycespace.Platform.Api.Business.Integration.Export;

public sealed record ExportRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ExportResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
