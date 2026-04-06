namespace Whycespace.Platform.Api.Intelligence.Observability.Diagnostic;

public sealed record DiagnosticRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DiagnosticResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
