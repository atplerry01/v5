namespace Whycespace.Platform.Api.Business.Billing.Statement;

public sealed record StatementRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StatementResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
