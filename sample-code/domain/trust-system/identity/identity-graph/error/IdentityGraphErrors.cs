namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public static class IdentityGraphErrors
{
    public static DomainException NotFound(Guid graphId)
        => new("GRAPH_NOT_FOUND", $"Identity graph '{graphId}' was not found.");

    public static DomainException LinkAlreadyExists(Guid sourceId, Guid targetId)
        => new("GRAPH_LINK_EXISTS", $"Link between '{sourceId}' and '{targetId}' already exists.");

    public static DomainException Closed(Guid graphId)
        => new("GRAPH_CLOSED", $"Identity graph '{graphId}' is closed.");
}
