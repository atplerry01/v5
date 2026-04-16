using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Api;

/// <summary>
/// phase1-gate-api-edge: direct unit test for the ConcurrencyConflictException
/// → 409 ProblemDetails handler. Exercises the IExceptionHandler in
/// isolation against a DefaultHttpContext — no WebApplicationFactory, no
/// real HTTP, no test host.
/// </summary>
public sealed class ConcurrencyConflictExceptionHandlerTest
{
    [Fact]
    public async Task Handler_writes_409_problem_details_for_concurrency_conflict()
    {
        var handler = new ConcurrencyConflictExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-abc-123");
        var aggregateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ex = new ConcurrencyConflictException(aggregateId, expectedVersion: 5, actualVersion: 7);

        var handled = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("urn:whyce:error:concurrency-conflict", root.GetProperty("type").GetString());
        Assert.Equal("Concurrent modification conflict", root.GetProperty("title").GetString());
        Assert.Equal(409, root.GetProperty("status").GetInt32());
        Assert.Equal(
            "The aggregate was modified by another writer. Reload and retry the operation.",
            root.GetProperty("detail").GetString());
        Assert.Equal(aggregateId.ToString(), root.GetProperty("aggregateId").GetString());
        Assert.Equal(5, root.GetProperty("expectedVersion").GetInt32());
        Assert.Equal(7, root.GetProperty("actualVersion").GetInt32());
        Assert.Equal("trace-abc-123", root.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task Handler_returns_false_for_unrelated_exceptions()
    {
        var handler = new ConcurrencyConflictExceptionHandler();
        var (httpContext, _) = NewContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("unrelated"),
            CancellationToken.None);

        Assert.False(handled);
        // Status code must NOT have been touched — still the default 200
        // until something else handles or the pipeline produces 500.
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Handler_uses_HttpContext_TraceIdentifier_for_correlation_id()
    {
        var handler = new ConcurrencyConflictExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-xyz");
        await handler.TryHandleAsync(
            httpContext,
            new ConcurrencyConflictException(Guid.Empty, 0, 1),
            CancellationToken.None);

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("trace-xyz", doc.RootElement.GetProperty("correlationId").GetString());
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
