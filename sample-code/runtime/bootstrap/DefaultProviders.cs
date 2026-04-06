using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Runtime.Bootstrap;

/// <summary>
/// Default pass-through authorization provider.
/// Allows all commands. Replace with a real provider for production.
/// </summary>
public sealed class AllowAllAuthorizationProvider : IAuthorizationProvider
{
    public Task<AuthorizationDecision> AuthorizeAsync(
        string commandType,
        string? whyceId,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(AuthorizationDecision.Allowed());
    }
}

/// <summary>
/// Default pass-through execution guard.
/// Allows all commands to execute. Replace with a real guard for production.
/// </summary>
public sealed class AllowAllExecutionGuard : IExecutionGuard
{
    public Task<bool> CanExecuteAsync(
        string commandType,
        string correlationId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
