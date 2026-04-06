using Whycespace.Platform.Middleware;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Platform.Adapters;

public sealed class ProjectionAdapter
{
    private readonly IProjectionQuerySource _querySource;

    public ProjectionAdapter(IProjectionQuerySource querySource)
    {
        _querySource = querySource;
    }

    public async Task<ApiResponse> QueryAsync<T>(
        string projectionName,
        IReadOnlyDictionary<string, object> parameters,
        string? traceId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _querySource.QueryAsync<T>(projectionName, parameters, cancellationToken);
        return ApiResponse.Ok(result, traceId);
    }

    public async Task<ApiResponse> QueryListAsync<T>(
        string projectionName,
        IReadOnlyDictionary<string, object> parameters,
        string? traceId = null,
        CancellationToken cancellationToken = default)
    {
        var results = await _querySource.QueryListAsync<T>(projectionName, parameters, cancellationToken);
        return ApiResponse.Ok(results, traceId);
    }
}
