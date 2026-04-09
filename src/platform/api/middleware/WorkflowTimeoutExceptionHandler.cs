using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): maps
/// <see cref="WorkflowTimeoutException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE REFUSAL
/// edge for workflow per-step / execution-level deadline expiry — the
/// engine-side counterpart to <see cref="WorkflowSaturatedException"/>
/// (which covers the admission-gate side).
///
/// Mirrors the precedents set by
/// <c>PolicyEvaluationUnavailableExceptionHandler</c>,
/// <c>OutboxSaturatedExceptionHandler</c>,
/// <c>WorkflowSaturatedExceptionHandler</c>,
/// <c>ChainAnchorWaitTimeoutExceptionHandler</c>, and
/// <c>ChainAnchorUnavailableExceptionHandler</c>:
///
///   - No layer between <c>T1MWorkflowEngine</c> and this handler is
///     permitted to catch <see cref="WorkflowTimeoutException"/> — it
///     must travel untouched from the throw site to here.
///   - The handler matches ONLY the typed exception; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from the
///     exception or the request context.
/// </summary>
public sealed class WorkflowTimeoutExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:workflow-timeout";
    private const string Title = "Workflow timeout";
    private const string Detail =
        "The workflow exceeded its declared per-step or execution-level deadline. " +
        "New work is refused until the workflow recovers. " +
        "Retry the request after the interval indicated by the Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not WorkflowTimeoutException timeout) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = Detail,
        };
        problem.Extensions["kind"] = timeout.Kind;
        if (timeout.StepName is not null)
            problem.Extensions["stepName"] = timeout.StepName;
        problem.Extensions["timeoutMs"] = timeout.TimeoutMs;
        problem.Extensions["retryAfterSeconds"] = timeout.RetryAfterSeconds;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.RetryAfter = timeout.RetryAfterSeconds.ToString();
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
