using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class MiddlewarePipeline : IMiddlewareInspector
{
    private readonly List<IMiddleware> _middlewares = new();

    internal IReadOnlyList<IMiddleware> Middlewares => _middlewares.AsReadOnly();

    public void Add(IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _middlewares.Add(middleware);
    }

    public MiddlewareDelegate Build(MiddlewareDelegate terminal)
    {
        var pipeline = terminal;

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = context => middleware.InvokeAsync(context, next);
        }

        return pipeline;
    }

    public IReadOnlyList<MiddlewareMetadata> GetMiddlewareMetadata()
    {
        return _middlewares.Select((m, i) => new MiddlewareMetadata
        {
            Name = m.GetType().Name.Replace("Middleware", ""),
            TypeName = m.GetType().FullName ?? m.GetType().Name,
            Order = i
        }).ToList().AsReadOnly();
    }
}
