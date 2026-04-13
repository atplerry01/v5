using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): maps
/// <see cref="OutboxSaturatedException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE REFUSAL
/// edge for the outbox saturation path.
///
/// Mirrors the phase1-gate-api-edge precedent set by
/// <c>ConcurrencyConflictExceptionHandler</c> and the phase1.5-S5.2.1
/// / PC-2 precedent set by <c>PolicyEvaluationUnavailableExceptionHandler</c>:
///
///   - No layer between <c>PostgresOutboxAdapter</c> and this handler
///     is permitted to catch <see cref="OutboxSaturatedException"/> —
///     it must travel untouched from the throw site to here. This
///     guarantees that an over-watermark outbox refuses the command at
///     the edge instead of silently dropping events downstream.
///   - The handler matches ONLY the typed exception; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from the
///     exception or the request context.
/// </summary>
public sealed class OutboxSaturatedExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:outbox-saturated";
    private const string Title = "Outbox temporarily saturated";
    private const string Detail =
        "The transactional outbox is at or above its declared high-water-mark. " +
        "New work is refused until the publisher loop drains the backlog. " +
        "Retry the request after the interval indicated by the Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not OutboxSaturatedException saturated) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = Detail,
        };
        problem.Extensions["observedDepth"] = saturated.ObservedDepth;
        problem.Extensions["highWaterMark"] = saturated.HighWaterMark;
        problem.Extensions["retryAfterSeconds"] = saturated.RetryAfterSeconds;
        // phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01):
        // surface the refusal reason so operators can distinguish
        // "high_water_mark" (canonical PC-3 saturation) from
        // "snapshot_stale" (fail-safe refusal because the
        // OutboxDepthSampler observation has aged past
        // OutboxOptions.SnapshotMaxAgeSeconds — closes H19). Same
        // family, same status, same Retry-After header.
        problem.Extensions["reason"] = saturated.Reason;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.RetryAfter = saturated.RetryAfterSeconds.ToString();
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
