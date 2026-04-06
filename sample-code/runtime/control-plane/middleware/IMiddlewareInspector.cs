namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed record MiddlewareMetadata
{
    public required string Name { get; init; }
    public required string TypeName { get; init; }
    public required int Order { get; init; }
}

public interface IMiddlewareInspector
{
    IReadOnlyList<MiddlewareMetadata> GetMiddlewareMetadata();
}
