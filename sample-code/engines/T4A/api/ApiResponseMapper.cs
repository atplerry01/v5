namespace Whycespace.Engines.T4A.Api;

public sealed class ApiResponseMapper
{
    public ApiResponse Map(object result, int statusCode = 200)
    {
        return new ApiResponse(statusCode, result);
    }
}

public sealed record ApiResponse(int StatusCode, object Body);
