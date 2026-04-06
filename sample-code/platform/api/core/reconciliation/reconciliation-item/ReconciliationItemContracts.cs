namespace Whycespace.Platform.Api.Core.Reconciliation.ReconciliationItem;

public sealed record ReconciliationItemRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReconciliationItemResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
