using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Whycespace.Tests.Integration.Chaos;

/// <summary>
/// R5.C.2 Phase 2 — in-memory end-to-end harness for a single chaos loop.
/// Wraps a canonical-exception invocation + handler execution inside a
/// listened <see cref="ActivitySource"/> so the test can assert the full
/// runtime-side signal chain:
///
/// <list type="bullet">
///   <item>the canonical exception fires,</item>
///   <item>the canonical handler returns <c>true</c>,</item>
///   <item>the response carries the canonical HTTP status + ProblemDetails type URI,</item>
///   <item>the wrapping <see cref="Activity"/> records <see cref="ActivityStatusCode.Error"/>
///   with the exception type name as the failure-reason tag.</item>
/// </list>
///
/// <para>Metric-listener capture + log-scope capture are NOT part of this
/// harness by design — R4.A / R-TRACE-LOG-CORRELATION-01 already pin the
/// metric + log contracts in isolation. Folding them in here would double
/// the complexity without adding independent coverage. Phase 3 (live
/// infrastructure) exercises the metric + alert observation end-to-end
/// against a running Prometheus.</para>
/// </summary>
internal sealed class ChaosLoopHarness : IAsyncDisposable
{
    public const string HarnessSourceName = "Whycespace.Tests.Chaos.LoopHarness";

    private readonly ActivitySource _source;
    private readonly ActivityListener _listener;
    private readonly List<Activity> _activities = new();

    public ChaosLoopHarness()
    {
        _source = new ActivitySource(HarnessSourceName, "1.0.0");
        _listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == HarnessSourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _activities.Add(a),
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public IReadOnlyList<Activity> CapturedActivities => _activities;

    /// <summary>
    /// Run the loop end-to-end:
    /// 1. Start a wrapping activity (simulating the runtime's canonical span).
    /// 2. Invoke the supplied fault action — it throws the canonical exception.
    /// 3. Catch the exception, mark the activity Error with the exception type
    ///    name as the failure-reason tag (mirrors the runtime's own
    ///    instrumentation pattern — see `SystemIntentDispatcher.DispatchAsync`).
    /// 4. Hand the exception to the provided handler against a fresh
    ///    <see cref="DefaultHttpContext"/>; capture the response shape.
    /// </summary>
    public async Task<ChaosLoopProof> RunAsync(
        string wrappingSpanName,
        Func<Task> faultAction,
        IExceptionHandler handler,
        string traceId = "chaos-harness-trace")
    {
        Exception? capturedException = null;
        Activity? wrapping;
        using (wrapping = _source.StartActivity(wrappingSpanName, ActivityKind.Internal))
        {
            try
            {
                await faultAction();
            }
            catch (Exception ex)
            {
                capturedException = ex;
                wrapping?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
                wrapping?.SetTag("whyce.failure_reason", ex.GetType().Name);
            }
        }

        if (capturedException is null)
            throw new InvalidOperationException(
                "Fault action did not throw. Chaos loop requires the canonical exception to fire.");

        var (httpContext, body) = NewHttpContext(traceId);
        var handled = await handler.TryHandleAsync(httpContext, capturedException, CancellationToken.None);

        return new ChaosLoopProof
        {
            ExceptionType = capturedException.GetType().Name,
            HandlerHandled = handled,
            HttpStatus = httpContext.Response.StatusCode,
            ContentType = httpContext.Response.ContentType,
            ResponseBody = ReadBody(body),
            WrappingActivity = wrapping,
        };
    }

    private static (HttpContext context, MemoryStream body) NewHttpContext(string traceId)
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

    public ValueTask DisposeAsync()
    {
        _listener.Dispose();
        _source.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Structured proof record returned from <see cref="ChaosLoopHarness.RunAsync"/>.
/// Every field is a link in the canonical chaos-observability-loop chain;
/// the per-loop test asserts on them.
/// </summary>
internal sealed record ChaosLoopProof
{
    public required string ExceptionType { get; init; }
    public required bool HandlerHandled { get; init; }
    public required int HttpStatus { get; init; }
    public required string? ContentType { get; init; }
    public required string ResponseBody { get; init; }
    public required Activity? WrappingActivity { get; init; }

    public string ProblemType()
    {
        using var doc = JsonDocument.Parse(ResponseBody);
        return doc.RootElement.GetProperty("type").GetString() ?? string.Empty;
    }
}
