using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// Maps any exception implementing <see cref="IDomainInvariantViolation"/>
/// (the canonical shared-layer marker for domain invariant violations —
/// <c>Whycespace.Domain.SharedKernel.Primitives.Kernel.DomainException</c>
/// being the primary implementer) to HTTP 400 Bad Request with an
/// RFC 7807 ProblemDetails payload. Domain invariant violations are a
/// caller-correctable condition, never a 500-class server fault.
///
/// <para>
/// <b>Layer discipline (DG-R5-01, 2026-04-20):</b> the handler depends
/// only on the shared-layer marker interface, not on the concrete
/// <c>Whycespace.Domain</c> exception type. This keeps
/// <c>platform_api</c> within its allowed-reference set (Shared +
/// Systems) per <c>dependency-check.sh</c>. The interface contract is
/// that every implementer is an <see cref="Exception"/> whose message
/// is safe to surface in a 400 response body (caller-correctable
/// invariant violation, not an internal fault).
/// </para>
///
/// Mirrors the <see cref="ConcurrencyConflictExceptionHandler"/> contract:
/// <list type="bullet">
///   <item>The handler matches ONLY exceptions implementing
///         <see cref="IDomainInvariantViolation"/>; any other type is
///         ignored (returns false) so default ASP.NET handling applies.</item>
///   <item>The payload is deterministic: every field derives from the
///         exception or request context. No timestamps, no stack traces,
///         no internal type names.</item>
///   <item>The correlation id is taken from <see cref="HttpContext.TraceIdentifier"/>.</item>
/// </list>
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
        if (exception is not IDomainInvariantViolation) return false;

        var problem = new ProblemDetails
        {
            Type = TypeUri,
            Title = Title,
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
        };
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
