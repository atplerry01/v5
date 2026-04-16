using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// Maps <see cref="DomainException"/> to HTTP 400 Bad Request with an
/// RFC 7807 ProblemDetails payload. Domain invariant violations are a
/// caller-correctable condition, never a 500-class server fault.
///
/// Mirrors the <see cref="ConcurrencyConflictExceptionHandler"/> contract:
///
///   - The handler matches ONLY <see cref="DomainException"/>; any other
///     exception type is ignored (returns false) so default ASP.NET
///     handling applies.
///   - The payload is deterministic: every field derives from the
///     exception or request context. No timestamps, no stack traces,
///     no internal type names.
///   - The correlation id is taken from <see cref="HttpContext.TraceIdentifier"/>.
/// </summary>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    private const string TypeUri = "urn:whyce:error:domain-invariant";
    private const string Title = "Domain invariant violation";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domain) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status400BadRequest,
            Detail = domain.Message,
        };
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
