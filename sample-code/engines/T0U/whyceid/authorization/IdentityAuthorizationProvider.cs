using Whycespace.Engines.T0U.WhyceId.Authorization;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Identity-aware authorization provider that delegates to T0U WhyceId
/// AuthorizationEngine for identity command authorization decisions.
/// Implements IAuthorizationProvider consumed by AuthorizationMiddleware.
/// </summary>
public sealed class IdentityAuthorizationProvider : IAuthorizationProvider
{
    private readonly AuthorizationEngine _authorizationEngine;

    public IdentityAuthorizationProvider(AuthorizationEngine authorizationEngine)
    {
        _authorizationEngine = authorizationEngine ?? throw new ArgumentNullException(nameof(authorizationEngine));
    }

    public Task<AuthorizationDecision> AuthorizeAsync(
        string commandType,
        string? whyceId,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(whyceId))
        {
            return Task.FromResult(AuthorizationDecision.Denied(
                "No identity provided. Authentication required."));
        }

        var resource = ExtractResource(commandType);
        var action = ExtractAction(commandType);

        var result = _authorizationEngine.Authorize(
            new AuthorizeCommand(whyceId, resource, action));

        return Task.FromResult(result.IsAuthorized
            ? AuthorizationDecision.Allowed()
            : AuthorizationDecision.Denied(result.Reason));
    }

    private static string ExtractResource(string commandType)
    {
        var dotIndex = commandType.IndexOf('.');
        return dotIndex > 0 ? commandType[..dotIndex] : commandType;
    }

    private static string ExtractAction(string commandType)
    {
        var dotIndex = commandType.IndexOf('.');
        return dotIndex > 0 ? commandType[(dotIndex + 1)..] : "execute";
    }
}
