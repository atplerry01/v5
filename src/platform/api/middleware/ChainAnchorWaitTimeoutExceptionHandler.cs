using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Infrastructure.Chain;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): maps
/// <see cref="ChainAnchorWaitTimeoutException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE REFUSAL
/// edge for the chain-anchor commit-serializer wait timeout path.
///
/// Mirrors the precedents set by
/// <c>ConcurrencyConflictExceptionHandler</c>,
/// <c>PolicyEvaluationUnavailableExceptionHandler</c>,
/// <c>OutboxSaturatedExceptionHandler</c>, and
/// <c>WorkflowSaturatedExceptionHandler</c>:
///
///   - No layer between <c>ChainAnchorService</c> and this handler is
///     permitted to catch <see cref="ChainAnchorWaitTimeoutException"/> —
///     it must travel untouched from the throw site to here. This
///     guarantees that a stuck chain holder refuses the command at the
///     edge instead of hanging the request indefinitely.
///   - The handler matches ONLY the typed exception; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from the
///     exception or the request context.
/// </summary>
public sealed class ChainAnchorWaitTimeoutExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:chain-anchor-wait-timeout";
    private const string Title = "Chain anchor temporarily unavailable";
    private const string Detail =
        "The chain anchor commit serializer did not become available within the declared wait timeout. " +
        "New work is refused until the in-flight chain commit completes. " +
        "Retry the request after the interval indicated by the Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ChainAnchorWaitTimeoutException timeout) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = Detail,
        };
        problem.Extensions["waitTimeoutMs"] = timeout.WaitTimeoutMs;
        problem.Extensions["retryAfterSeconds"] = timeout.RetryAfterSeconds;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.RetryAfter = timeout.RetryAfterSeconds.ToString();
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
