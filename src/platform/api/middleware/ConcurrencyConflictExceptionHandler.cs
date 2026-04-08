using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Platform.Api.Middleware;

/// <summary>
/// phase1-gate-api-edge: maps <see cref="ConcurrencyConflictException"/> to
/// HTTP 409 Conflict with an RFC 7807 ProblemDetails payload.
///
/// This is the SINGLE seam where the H8b optimistic-concurrency exception
/// crosses the application/HTTP boundary. By contract (locked):
///
///   - No layer between PostgresEventStoreAdapter and this handler is
///     permitted to catch ConcurrencyConflictException — it must travel
///     untouched from the throw site to here.
///   - The handler matches ONLY ConcurrencyConflictException; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from either the
///     exception or the request context. No timestamps, no stack traces,
///     no internal type names.
///   - The correlation id is taken from <see cref="HttpContext.TraceIdentifier"/>
///     — never generated locally.
/// </summary>
public sealed class ConcurrencyConflictExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:concurrency-conflict";
    private const string Title = "Concurrent modification conflict";
    private const string Detail =
        "The aggregate was modified by another writer. Reload and retry the operation.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyConflictException conflict) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status409Conflict,
            Detail = Detail,
        };
        problem.Extensions["aggregateId"] = conflict.AggregateId;
        problem.Extensions["expectedVersion"] = conflict.ExpectedVersion;
        problem.Extensions["actualVersion"] = conflict.ActualVersion;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        // Pass contentType explicitly so WriteAsJsonAsync does not overwrite
        // it with the default "application/json; charset=utf-8".
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
