using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Infrastructure.Policy;

namespace Whyce.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): maps
/// <see cref="PolicyEvaluationUnavailableException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE REFUSAL
/// edge for the policy hot path.
///
/// Mirrors the phase1-gate-api-edge precedent set by
/// <c>ConcurrencyConflictExceptionHandler</c>:
///
///   - No layer between <c>OpaPolicyEvaluator</c> and this handler is
///     permitted to catch <see cref="PolicyEvaluationUnavailableException"/>
///     — it must travel untouched from the throw site to here. This
///     guarantees policy primacy ($8): a transient OPA failure is never
///     converted to an implicit allow.
///   - The handler matches ONLY the typed exception; any other exception
///     type is ignored (returns false) so default ASP.NET handling
///     applies.
///   - The payload is deterministic: every field derives from either the
///     exception or the request context.
/// </summary>
public sealed class PolicyEvaluationUnavailableExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:policy-evaluation-unavailable";
    private const string Title = "Policy evaluation temporarily unavailable";
    private const string Detail =
        "External policy evaluation could not produce a decision within the declared envelope. " +
        "Retry the request after the interval indicated by the Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not PolicyEvaluationUnavailableException unavailable) return false;

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
