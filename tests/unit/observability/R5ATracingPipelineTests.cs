using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Whycespace.Runtime.Observability;

namespace Whycespace.Tests.Unit.Observability;

/// <summary>
/// R5.A — validator tests for the tracing pipeline:
///
/// <list type="bullet">
///   <item>R-TRACE-SOURCE-VOCABULARY-01 — canonical
///   <see cref="WhyceActivitySources"/> constants exist; instrumented seams
///   reference them (no raw string drift).</item>
///   <item>R-TRACE-EXPORTER-OTEL-01 — Host csproj declares the required
///   OTEL packages; TracingInfrastructureModule registers both canonical
///   sources; Jaeger service is wired in docker-compose.</item>
///   <item>R-TRACE-CORRELATION-BRIDGE-01 — the Api-layer middleware uses
///   the canonical <c>whyce.correlation_id</c> attribute key (drift guard
///   for the inlined duplication per DG-R5-01).</item>
///   <item>R-TRACE-LAYER-DISCIPLINE-01 — runtime csproj carries NO
///   OpenTelemetry NuGet reference.</item>
/// </list>
/// </summary>
public sealed class R5ATracingPipelineTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void Canonical_activity_source_names_are_declared()
    {
        Assert.Equal("Whycespace.Runtime.ControlPlane", WhyceActivitySources.ControlPlaneName);
        Assert.Equal("Whycespace.Runtime.Admin", WhyceActivitySources.AdminName);
    }

    [Fact]
    public void Canonical_span_names_are_low_cardinality_constants()
    {
        Assert.Equal("runtime.command.dispatch", WhyceActivitySources.Spans.CommandDispatch);
        Assert.Equal("runtime.admin.operator_action", WhyceActivitySources.Spans.OperatorAction);
    }

    [Fact]
    public void SystemIntentDispatcher_uses_canonical_control_plane_source_and_dispatch_span()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "dispatcher", "SystemIntentDispatcher.cs");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.ControlPlane.StartActivity", text);
        Assert.Contains("WhyceActivitySources.Spans.CommandDispatch", text);
        Assert.Contains("WhyceActivitySources.Attributes.CommandType", text);
        Assert.Contains("WhyceActivitySources.Attributes.Classification", text);
        Assert.Contains("WhyceActivitySources.Attributes.Outcome", text);
    }

    [Fact]
    public void OperatorActionAuditRecorder_uses_canonical_admin_source_and_operator_action_span()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "control-plane", "admin", "OperatorActionAuditRecorder.cs");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);

        Assert.Contains("WhyceActivitySources.Admin.StartActivity", text);
        Assert.Contains("WhyceActivitySources.Spans.OperatorAction", text);
        Assert.Contains("WhyceActivitySources.Attributes.ActionType", text);
        Assert.Contains("WhyceActivitySources.Attributes.Outcome", text);
    }

    [Fact]
    public void Tracing_infrastructure_module_registers_both_canonical_sources()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "host", "composition",
            "infrastructure", "observability", "TracingInfrastructureModule.cs");
        Assert.True(File.Exists(path), $"TracingInfrastructureModule.cs missing at {path}.");
        var text = File.ReadAllText(path);

        Assert.Contains("AddOpenTelemetry", text);
        Assert.Contains(".WithTracing", text);
        Assert.Contains("WhyceActivitySources.ControlPlaneName", text);
        Assert.Contains("WhyceActivitySources.AdminName", text);
        Assert.Contains("AddOtlpExporter", text);
        Assert.Contains("AddAspNetCoreInstrumentation", text);
    }

    [Fact]
    public void Host_csproj_declares_required_otel_packages()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "host", "Whycespace.Host.csproj");
        var text = File.ReadAllText(path);

        Assert.Contains("OpenTelemetry.Extensions.Hosting", text);
        Assert.Contains("OpenTelemetry.Instrumentation.AspNetCore", text);
        Assert.Contains("OpenTelemetry.Instrumentation.Http", text);
        Assert.Contains("OpenTelemetry.Exporter.OpenTelemetryProtocol", text);
    }

    [Fact]
    public void Runtime_csproj_does_not_reference_opentelemetry()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "Whycespace.Runtime.csproj");
        var text = File.ReadAllText(path);

        Assert.DoesNotContain("OpenTelemetry", text);
    }

    // R-TRACE-LAYER-DISCIPLINE-01 (strengthened 2026-04-20): the prior test
    // substring-scanned only for "OpenTelemetry", which allowed `prometheus-net`
    // to slip into the runtime csproj via HttpMetricsMiddleware. This test
    // pins the runtime csproj to a closed allow-list of PackageReference
    // includes so any new NuGet dependency is a deliberate, reviewed change.
    private static readonly string[] RuntimePackageAllowList =
    {
        "Microsoft.AspNetCore.Http.Abstractions",
        "Microsoft.Extensions.DependencyInjection.Abstractions",
        "Microsoft.Extensions.Logging.Abstractions",
        "System.Threading.RateLimiting"
    };

    [Fact]
    public void Runtime_csproj_package_references_are_on_allow_list()
    {
        var path = Path.Combine(RepoRoot, "src", "runtime", "Whycespace.Runtime.csproj");
        var text = File.ReadAllText(path);

        var includes = Regex
            .Matches(text, "<PackageReference\\s+Include=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var disallowed = includes.Except(RuntimePackageAllowList).ToArray();
        Assert.True(
            disallowed.Length == 0,
            $"Runtime csproj references package(s) outside the allow-list: " +
            $"{string.Join(", ", disallowed)}. Allow-list = " +
            $"{string.Join(", ", RuntimePackageAllowList)}. If an addition is " +
            "genuinely required, update RuntimePackageAllowList + the " +
            "R-TRACE-LAYER-DISCIPLINE-01 guard rule in the same change.");
    }

    [Fact]
    public void Runtime_csproj_does_not_reference_prometheus_net()
    {
        // Explicit deny — the package that motivated the allow-list hardening.
        // Matches only actual PackageReference nodes so the rule can still
        // be cited in free-text comments without tripping this test.
        var path = Path.Combine(RepoRoot, "src", "runtime", "Whycespace.Runtime.csproj");
        var text = File.ReadAllText(path);

        var includes = Regex
            .Matches(text, "<PackageReference\\s+Include=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToArray();

        Assert.DoesNotContain(includes, include =>
            include.Equals("prometheus-net", StringComparison.OrdinalIgnoreCase) ||
            include.StartsWith("prometheus-net.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void HttpMetricsMiddleware_lives_in_platform_host_not_runtime()
    {
        // R-TRACE-LAYER-DISCIPLINE-01: prometheus-net is infra-layer. The HTTP
        // metrics middleware that uses it MUST live under src/platform/host/,
        // not src/runtime/.
        var runtimePath = Path.Combine(RepoRoot, "src", "runtime", "observability", "HttpMetricsMiddleware.cs");
        var hostPath = Path.Combine(RepoRoot, "src", "platform", "host", "observability", "HttpMetricsMiddleware.cs");

        Assert.False(File.Exists(runtimePath), $"HttpMetricsMiddleware must not live in runtime; found at {runtimePath}.");
        Assert.True(File.Exists(hostPath), $"HttpMetricsMiddleware must live at {hostPath}.");

        var text = File.ReadAllText(hostPath);
        Assert.Contains("namespace Whycespace.Platform.Host.Observability", text);
    }

    [Fact]
    public void Trace_correlation_middleware_uses_canonical_whyce_correlation_id_attribute_key()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "api", "middleware", "TraceCorrelationMiddleware.cs");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);

        // The middleware inlines the string "whyce.correlation_id" because
        // platform_api does not reference the runtime layer. This test pins
        // the inlined value to the canonical runtime-layer constant so they
        // never drift.
        Assert.Contains("\"whyce.correlation_id\"", text);
        Assert.Contains("traceresponse", text);
        Assert.Equal("whyce.correlation_id", WhyceActivitySources.Attributes.CorrelationId);
    }

    [Fact]
    public void Docker_compose_wires_jaeger_service_with_otlp_port()
    {
        var path = Path.Combine(RepoRoot, "infrastructure", "deployment", "docker-compose.yml");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);

        Assert.Contains("jaegertracing/all-in-one", text);
        Assert.Contains("4317:4317", text);
        Assert.Contains("16686:16686", text);
    }

    [Fact]
    public void Program_cs_registers_trace_correlation_middleware_after_correlation_middleware()
    {
        var path = Path.Combine(RepoRoot, "src", "platform", "host", "Program.cs");
        var text = File.ReadAllText(path);
        var correlationIdx = text.IndexOf("CorrelationIdMiddleware", StringComparison.Ordinal);
        var traceIdx = text.IndexOf("TraceCorrelationMiddleware", StringComparison.Ordinal);
        Assert.True(correlationIdx > 0, "CorrelationIdMiddleware not registered in Program.cs.");
        Assert.True(traceIdx > 0, "TraceCorrelationMiddleware not registered in Program.cs.");
        Assert.True(correlationIdx < traceIdx,
            "TraceCorrelationMiddleware must run AFTER CorrelationIdMiddleware so the correlation id is on HttpContext.Items.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Whycespace.sln")) &&
               !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }
}
