using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Api;

/// <summary>
/// R5.B (Phase 2) / R-CHAOS-PROOF-EXISTS-01 — certifies that
/// <see cref="WorkflowSaturatedExceptionHandler"/> maps the canonical
/// admission-gate saturation fault to HTTP 503 + canonical
/// ProblemDetails + <c>Retry-After</c> header, as declared in the
/// failure-mode catalog. Direct test against a <c>DefaultHttpContext</c>;
/// no WebApplicationFactory, no infrastructure.
/// </summary>
public sealed class WorkflowSaturatedExceptionHandlerTest
{
    [Fact]
    public async Task Handler_writes_503_problem_details_for_saturation_fault()
    {
        var handler = new WorkflowSaturatedExceptionHandler();
        var (httpContext, body) = NewContext(traceId: "trace-ws-001");
        var ex = new WorkflowSaturatedException(
            workflowName: "payout-execute",
            partition: "workflow",
            retryAfterSeconds: 5);

        var handled = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);
        Assert.Equal("5", httpContext.Response.Headers.RetryAfter.ToString());

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("urn:whyce:error:workflow-saturated", root.GetProperty("type").GetString());
        Assert.Equal("Workflow temporarily saturated", root.GetProperty("title").GetString());
        Assert.Equal(503, root.GetProperty("status").GetInt32());
        Assert.Equal("payout-execute", root.GetProperty("workflowName").GetString());
        Assert.Equal("workflow", root.GetProperty("partition").GetString());
        Assert.Equal(5, root.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal("trace-ws-001", root.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task Handler_returns_false_for_unrelated_exceptions()
    {
        var handler = new WorkflowSaturatedExceptionHandler();
        var (httpContext, _) = NewContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("unrelated"),
            CancellationToken.None);

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.True(string.IsNullOrEmpty(httpContext.Response.Headers.RetryAfter.ToString()));
    }

    [Fact]
    public async Task Handler_surfaces_tenant_partition_distinct_from_workflow_partition()
    {
        var handler = new WorkflowSaturatedExceptionHandler();
        var (httpContext, body) = NewContext();
        var ex = new WorkflowSaturatedException(
            workflowName: "settle-tx",
            partition: "tenant",
            retryAfterSeconds: 10);

        await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        var json = ReadBody(body);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("tenant", doc.RootElement.GetProperty("partition").GetString());
        Assert.Equal(10, doc.RootElement.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal("10", httpContext.Response.Headers.RetryAfter.ToString());
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
