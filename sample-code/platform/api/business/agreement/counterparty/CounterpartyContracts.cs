namespace Whycespace.Platform.Api.Business.Agreement.Counterparty;

public sealed record CounterpartyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CounterpartyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
