namespace Whycespace.Engines.T4A.Api;

public sealed class ApiCommandAdapter
{
    public ApiCommand Adapt(string requestPath, string method, string body)
    {
        return new ApiCommand(requestPath, method, body);
    }
}

public sealed record ApiCommand(string Path, string Method, string Body);
