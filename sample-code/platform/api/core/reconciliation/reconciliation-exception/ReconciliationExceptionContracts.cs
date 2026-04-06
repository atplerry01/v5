namespace Whycespace.Platform.Api.Core.Reconciliation.ReconciliationException;

public sealed record ReconciliationExceptionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReconciliationExceptionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
