namespace Whycespace.Shared.Contracts.Economic.Routing.Path;

public sealed record RoutingPathReadModel
{
    public Guid PathId { get; init; }
    public string PathType { get; init; } = string.Empty;
    public string Conditions { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset DefinedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? DisabledAt { get; init; }
}
