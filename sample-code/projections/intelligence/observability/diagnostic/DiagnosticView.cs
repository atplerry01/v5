namespace Whycespace.Projections.Intelligence.Observability.Diagnostic;

public sealed record DiagnosticView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
