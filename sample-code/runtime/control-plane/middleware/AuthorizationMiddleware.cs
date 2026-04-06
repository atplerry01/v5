using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class AuthorizationMiddleware : IMiddleware
{
    private readonly IAuthorizationProvider _provider;

    public AuthorizationMiddleware(IAuthorizationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var envelope = context.Envelope;

        var decision = await _provider.AuthorizeAsync(
            envelope.CommandType,
            envelope.Metadata.WhyceId,
            envelope.Metadata.Headers,
            context.CancellationToken);

        if (!decision.IsAuthorized)
        {
            return CommandResult.Fail(
                envelope.CommandId,
                decision.Reason ?? "Authorization denied.",
                "AUTHORIZATION_DENIED");
        }

        context.Set(ContextKeys.AuthorizationDecision, decision);

        return await next(context);
    }

    public static class ContextKeys
    {
        public const string AuthorizationDecision = "Security.AuthorizationDecision";
    }
}

public interface IAuthorizationProvider
{
    Task<AuthorizationDecision> AuthorizeAsync(
        string commandType,
        string? whyceId,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken);
}

public sealed record AuthorizationDecision
{
    public required bool IsAuthorized { get; init; }
    public string? Reason { get; init; }

    public static AuthorizationDecision Allowed() => new() { IsAuthorized = true };
    public static AuthorizationDecision Denied(string reason) => new() { IsAuthorized = false, Reason = reason };
}
