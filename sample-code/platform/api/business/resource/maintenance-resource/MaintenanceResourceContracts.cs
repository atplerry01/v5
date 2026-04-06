namespace Whycespace.Platform.Api.Business.Resource.MaintenanceResource;

public sealed record MaintenanceResourceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MaintenanceResourceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
