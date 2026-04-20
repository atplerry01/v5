using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Api;

/// <summary>
/// R5.B (Phase 2) / R-CHAOS-PROOF-EXISTS-01 — certifies that
/// <see cref="WorkflowTimeoutExceptionHandler"/> maps both per-step and
/// per-execution timeout faults to HTTP 503 + canonical ProblemDetails +
/// <c>Retry-After</c> header. Engine-side counterpart to the admission-
/// gate saturation handler test.
/// </summary>
public sealed class WorkflowTimeoutExceptionHandlerTest
{
    [Fact]
    public async Task Handler_writes_503_problem_details_for_step_timeout()
    {
        var handler = new WorkflowTimeoutExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-wt-step");
        var ex = new WorkflowTimeoutException(
            kind: "step",
            stepName: "call-provider",
            timeoutMs: 2000,
            retryAfterSeconds: 5,
            message: "Step 'call-provider' exceeded 2000ms");

        var handled = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);
        Assert.Equal("5", httpContext.Response.Headers.RetryAfter.ToString());

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("urn:whyce:error:workflow-timeout", root.GetProperty("type").GetString());
        Assert.Equal("Workflow timeout", root.GetProperty("title").GetString());
        Assert.Equal(503, root.GetProperty("status").GetInt32());
        Assert.Equal("step", root.GetProperty("kind").GetString());
        Assert.Equal("call-provider", root.GetProperty("stepName").GetString());
        Assert.Equal(2000, root.GetProperty("timeoutMs").GetInt32());
        Assert.Equal(5, root.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal("trace-wt-step", root.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task Handler_writes_503_without_step_name_for_execution_timeout()
    {
        var handler = new WorkflowTimeoutExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-wt-exec");
        var ex = new WorkflowTimeoutException(
            kind: "execution",
            stepName: null,
            timeoutMs: 60000,
            retryAfterSeconds: 30,
            message: "Execution exceeded 60000ms");

        await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("execution", root.GetProperty("kind").GetString());
        Assert.False(root.TryGetProperty("stepName", out _));
        Assert.Equal(60000, root.GetProperty("timeoutMs").GetInt32());
        Assert.Equal("30", httpContext.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task Handler_returns_false_for_unrelated_exceptions()
    {
        var handler = new WorkflowTimeoutExceptionHandler();
        var (httpContext, _) = NewContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new OperationCanceledException("caller cancelled"),
            CancellationToken.None);

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
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
