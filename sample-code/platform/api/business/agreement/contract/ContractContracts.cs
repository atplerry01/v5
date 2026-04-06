namespace Whycespace.Platform.Api.Business.Agreement.Contract;

public sealed record ContractRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ContractResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
