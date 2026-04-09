using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Infrastructure.Chain;

namespace Whyce.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): maps
/// <see cref="ChainAnchorUnavailableException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE REFUSAL
/// edge for chain-store transport failures and breaker-open refusals —
/// the holder-side counterpart to
/// <see cref="ChainAnchorWaitTimeoutExceptionHandler"/> (TC-2's
/// wait-side handler).
///
/// Mirrors the precedents set by
/// <c>PolicyEvaluationUnavailableExceptionHandler</c>,
/// <c>OutboxSaturatedExceptionHandler</c>,
/// <c>WorkflowSaturatedExceptionHandler</c>, and
/// <c>ChainAnchorWaitTimeoutExceptionHandler</c>:
///
///   - No layer between <c>WhyceChainPostgresAdapter</c> and this
///     handler is permitted to catch <see cref="ChainAnchorUnavailableException"/> —
///     it must travel untouched from the throw site to here.
///   - The handler matches ONLY the typed exception; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from the
///     exception or the request context.
/// </summary>
public sealed class ChainAnchorUnavailableExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:chain-anchor-unavailable";
    private const string Title = "Chain anchor temporarily unavailable";
    private const string Detail =
        "The chain-store adapter could not complete the chain commit. " +
        "Either the underlying transport failed or the adapter's circuit breaker is open. " +
        "New work is refused until the chain-store recovers. " +
        "Retry the request after the interval indicated by the Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ChainAnchorUnavailableException unavailable) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = Detail,
        };
        problem.Extensions["reason"] = unavailable.Reason;
        problem.Extensions["retryAfterSeconds"] = unavailable.RetryAfterSeconds;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.RetryAfter = unavailable.RetryAfterSeconds.ToString();
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
