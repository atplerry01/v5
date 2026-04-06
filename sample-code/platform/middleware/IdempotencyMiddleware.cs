namespace Whycespace.Platform.Middleware;

public sealed class IdempotencyMiddleware : IApiMiddleware
{
    private readonly IIdempotencyStore _store;

    public IdempotencyMiddleware(IIdempotencyStore store)
    {
        _store = store;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var idempotencyKey = request.Headers.GetValueOrDefault("Idempotency-Key");
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return await next(request);

        var cached = await _store.GetAsync(idempotencyKey);
        if (cached is not null)
            return cached;

        var response = await next(request);

        if (response.StatusCode < 500)
            await _store.SetAsync(idempotencyKey, response);

        return response;
    }
}

public interface IIdempotencyStore
{
    Task<ApiResponse?> GetAsync(string key);
    Task SetAsync(string key, ApiResponse response);
}
