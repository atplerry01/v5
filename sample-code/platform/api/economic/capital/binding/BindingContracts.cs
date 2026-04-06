namespace Whycespace.Platform.Api.Economic.Capital.Binding;

public sealed record BindingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BindingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
