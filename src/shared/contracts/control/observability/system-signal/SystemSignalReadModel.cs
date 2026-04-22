namespace Whycespace.Shared.Contracts.Control.Observability.SystemSignal;

public sealed record SystemSignalReadModel
{
    public Guid SignalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public bool IsDeprecated { get; init; }
}
