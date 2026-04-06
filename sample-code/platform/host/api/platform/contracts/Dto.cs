namespace Whycespace.Platform.Api.Platform.Contracts;

public sealed record PingCommandRequest
{
    public string? CommandId { get; init; }
    public required string AggregateId { get; init; }
    public required string Message { get; init; }
}
