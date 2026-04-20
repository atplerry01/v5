using Microsoft.AspNetCore.Http;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Runtime.Admin;

namespace Whycespace.Platform.Host.Adapters.Admin;

/// <summary>
/// R4.B — reads the current request's correlation id from the ambient HTTP
/// context's items bag, populated upstream by
/// <see cref="CorrelationIdMiddleware"/>. Singleton; resolves via
/// <see cref="IHttpContextAccessor"/>'s async-local storage.
///
/// <para>Returns <see cref="Guid.Empty"/> when invoked outside an HTTP
/// request (e.g. background workers) — callers on the admin surface run
/// inside HTTP requests, so this fallback is only exercised in degenerate
/// paths.</para>
/// </summary>
internal sealed class HttpRequestCorrelationAccessor : IRequestCorrelationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestCorrelationAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid Current
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return Guid.Empty;
            if (ctx.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var raw) && raw is Guid id)
                return id;
            return Guid.Empty;
        }
    }
}
