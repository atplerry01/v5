using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.3 / TC-9 (HOST-SHUTDOWN-DRAIN-01): replaces
/// <see cref="HttpContext.RequestAborted"/> for the duration of every
/// request with a linked <see cref="CancellationTokenSource"/> that
/// fires when EITHER the original client-disconnect token fires OR
/// <see cref="IHostApplicationLifetime.ApplicationStopping"/> fires.
///
/// Why this seam: TC-1 already established that the controller's
/// <c>CancellationToken</c> parameter (bound by ASP.NET model binding
/// to <c>RequestAborted</c>) flows through the system intent dispatcher,
/// the locked middleware pipeline, the runtime control plane, the
/// dispatcher, the event fabric, and into every Postgres / chain /
/// workflow / projection seam. Linking <c>RequestAborted</c> to
/// <c>ApplicationStopping</c> at the very edge means TC-9 inherits all
/// of that plumbing without touching a single downstream contract: the
/// instant the host begins to drain, every in-flight request observes
/// the same cancellation that TC-1 plumbed end-to-end.
///
/// The middleware is intentionally narrow: it does not introduce a new
/// timeout, does not introduce a new refusal class, and does not
/// touch the response shape. The declared
/// <c>HostOptions.ShutdownTimeout</c> remains the host-side wall-clock
/// ceiling; this middleware merely propagates the shutdown signal into
/// the request path so the in-flight pipeline can begin to wind down
/// cooperatively before that ceiling is reached.
/// </summary>
public sealed class HostShutdownLinkingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostApplicationLifetime _lifetime;

    public HostShutdownLinkingMiddleware(RequestDelegate next, IHostApplicationLifetime lifetime)
    {
        _next = next;
        _lifetime = lifetime;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalAborted = context.RequestAborted;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            originalAborted, _lifetime.ApplicationStopping);

        // Replace RequestAborted with the linked token. ASP.NET's
        // built-in CancellationToken model binder reads this property,
        // so the controller's `CancellationToken cancellationToken`
        // parameter automatically reflects the linked token without any
        // controller change.
        context.RequestAborted = linked.Token;
        try
        {
            await _next(context);
        }
        finally
        {
            // Restore so any downstream framework code that captured
            // the original token sees the original lifecycle. The
            // linked CTS is disposed by the using-block.
            context.RequestAborted = originalAborted;
        }
    }
}
