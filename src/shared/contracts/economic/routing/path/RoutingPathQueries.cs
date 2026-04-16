namespace Whycespace.Shared.Contracts.Economic.Routing.Path;

public sealed record GetRoutingPathByIdQuery(Guid PathId);

public sealed record ListRoutingPathsByStatusQuery(string Status);

public sealed record ListRoutingPathsByTypeQuery(string PathType);
