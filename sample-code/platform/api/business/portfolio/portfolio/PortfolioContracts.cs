namespace Whycespace.Platform.Api.Business.Portfolio.Portfolio;

public sealed record PortfolioRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PortfolioResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
