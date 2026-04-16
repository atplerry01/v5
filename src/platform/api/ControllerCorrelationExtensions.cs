using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Platform.Api.Middleware;

namespace Whycespace.Platform.Api;

/// <summary>
/// Closed-loop correlation helper. Returns the request's correlation id as
/// stamped by <see cref="CorrelationIdMiddleware"/>. Returns
/// <see cref="Guid.Empty"/> only when the middleware isn't wired (e.g. unit
/// tests with a bare <see cref="HttpContext"/>) — controllers should treat
/// that as the empty sentinel and pass it straight to <c>ApiResponse.Ok</c>.
/// </summary>
public static class ControllerCorrelationExtensions
{
    public static Guid RequestCorrelationId(this ControllerBase controller)
    {
        if (controller.HttpContext is { } ctx
            && ctx.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var raw)
            && raw is Guid id)
        {
            return id;
        }
        return Guid.Empty;
    }
}
