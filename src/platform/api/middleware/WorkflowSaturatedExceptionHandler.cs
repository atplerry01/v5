using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Api.Middleware;

/// <summary>
/// phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): maps
/// <see cref="WorkflowSaturatedException"/> to HTTP 503 Service
/// Unavailable with an RFC 7807 ProblemDetails payload and a
/// <c>Retry-After</c> header. This is the canonical RETRYABLE
/// REFUSAL edge for the workflow admission gate.
///
/// Mirrors the precedent set by
/// <c>ConcurrencyConflictExceptionHandler</c> (PC-2),
/// <c>PolicyEvaluationUnavailableExceptionHandler</c> (PC-2), and
/// <c>OutboxSaturatedExceptionHandler</c> (PC-3):
///
///   - No layer between <c>WorkflowAdmissionGate</c> and this
///     handler is permitted to catch <see cref="WorkflowSaturatedException"/>
///     — it must travel untouched from the throw site to here. The
///     workflow is refused at the gate; no engine step runs.
///   - The handler matches ONLY the typed exception; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic.
/// </summary>
public sealed class WorkflowSaturatedExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:workflow-saturated";
    private const string Title = "Workflow temporarily saturated";
    private const string Detail =
        "The runtime workflow admission gate is at or above its declared per-workflow-name " +
        "or per-tenant ceiling. Retry the request after the interval indicated by the " +
        "Retry-After header.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not WorkflowSaturatedException saturated) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = Detail,
        };
        problem.Extensions["workflowName"] = saturated.WorkflowName;
        problem.Extensions["partition"] = saturated.Partition;
        problem.Extensions["retryAfterSeconds"] = saturated.RetryAfterSeconds;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.RetryAfter = saturated.RetryAfterSeconds.ToString();
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
