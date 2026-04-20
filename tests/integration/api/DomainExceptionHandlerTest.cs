using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.Api;

/// <summary>
/// R5.B (Phase 2) / R-CHAOS-PROOF-EXISTS-01 — certifies that
/// <see cref="DomainExceptionHandler"/> maps every exception implementing
/// <see cref="IDomainInvariantViolation"/> to HTTP 400 + canonical
/// ProblemDetails. Validates the DG-R5-01 layer discipline: handler
/// matches by marker interface, not by concrete domain type, so
/// <c>platform_api</c> never reaches into <c>Whycespace.Domain</c>.
/// </summary>
public sealed class DomainExceptionHandlerTest
{
    [Fact]
    public async Task Handler_writes_400_problem_details_for_domain_exception()
    {
        var handler = new DomainExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-de-001");
        var ex = new DomainException("Amount must be positive.");

        var handled = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("urn:whyce:error:domain-invariant", root.GetProperty("type").GetString());
        Assert.Equal("Domain invariant violation", root.GetProperty("title").GetString());
        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.Equal("Amount must be positive.", root.GetProperty("detail").GetString());
        Assert.Equal("trace-de-001", root.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task Handler_matches_any_marker_interface_implementer_not_just_concrete_domain_exception()
    {
        var handler = new DomainExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-marker");
        var ex = new CustomInvariantViolation("Reservation exceeds balance.");

        var handled = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Reservation exceeds balance.", doc.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Handler_returns_false_for_unrelated_exceptions()
    {
        var handler = new DomainExceptionHandler();
        var (httpContext, _) = NewContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("not a domain invariant"),
            CancellationToken.None);

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    // Verifies the marker-interface design: any Exception implementing
    // IDomainInvariantViolation gets the 400 treatment, regardless of base
    // class. This is what lets platform_api stay independent of
    // Whycespace.Domain concrete types.
    private sealed class CustomInvariantViolation : Exception, IDomainInvariantViolation
    {
        public CustomInvariantViolation(string message) : base(message) { }
    }

    private static (HttpContext context, MemoryStream body) NewContext(string traceId = "trace-test")
    {
        var ctx = new DefaultHttpContext { TraceIdentifier = traceId };
        var body = new MemoryStream();
        ctx.Response.Body = body;
        return (ctx, body);
    }

    private static string ReadBody(MemoryStream body)
    {
        body.Position = 0;
        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
    }
}
