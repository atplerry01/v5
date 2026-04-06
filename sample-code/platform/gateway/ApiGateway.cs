using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Gateway;

public sealed class ApiGateway
{
    private readonly IReadOnlyList<IApiMiddleware> _middlewares;
    private readonly Func<ApiRequest, Task<ApiResponse>> _handler;

    public ApiGateway(IReadOnlyList<IApiMiddleware> middlewares, Func<ApiRequest, Task<ApiResponse>> handler)
    {
        _middlewares = middlewares;
        _handler = handler;
    }

    public Task<ApiResponse> ProcessAsync(ApiRequest request)
    {
        var pipeline = _middlewares
            .Reverse()
            .Aggregate(_handler, (next, middleware) =>
                req => middleware.InvokeAsync(req, next));

        return pipeline(request);
    }
}
