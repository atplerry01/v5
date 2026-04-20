using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Platform.Host.Bootstrap;
using Whycespace.Platform.Host.Composition;
using Whycespace.Platform.Host.Composition.Loader;
using Whycespace.Shared.Contracts.Infrastructure.Admission;

var builder = WebApplication.CreateBuilder(args);

// phase1.5-S5.2.3 / TC-9 (HOST-SHUTDOWN-DRAIN-01): declared graceful
// shutdown timeout. Pre-TC-9 the host inherited the .NET default
// (30s on .NET 8+, but undeclared and invisible to operators). TC-9
// makes the value canonical, externalised, and audit-visible. The
// configured ceiling is the wall-clock window the host gives in-flight
// requests to drain after IHostApplicationLifetime.ApplicationStopping
// fires; once it elapses, the runtime is forcibly stopped. The
// drain-side cancellation is wired separately below via the
// HostShutdownLinkingMiddleware which links HttpContext.RequestAborted
// to ApplicationStopping so the request CT path established by TC-1
// also fires on host shutdown.
var shutdownTimeoutSeconds = builder.Configuration.GetValue<int?>("Host:ShutdownTimeoutSeconds") ?? 30;
if (shutdownTimeoutSeconds < 1)
    throw new InvalidOperationException(
        $"Host:ShutdownTimeoutSeconds must be at least 1 (was {shutdownTimeoutSeconds}).");
builder.Services.Configure<Microsoft.Extensions.Hosting.HostOptions>(opts =>
{
    opts.ShutdownTimeout = TimeSpan.FromSeconds(shutdownTimeoutSeconds);
});

// --- Domain bootstrap modules (classification → context → domain) ---
// All per-domain wiring lives in BootstrapModuleCatalog. Program.cs holds zero domain knowledge.
foreach (var module in BootstrapModuleCatalog.All)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

// Modules are also DI-resolvable so registry factories can iterate them at resolution time.
foreach (var module in BootstrapModuleCatalog.All)
{
    builder.Services.AddSingleton<IDomainBootstrapModule>(module);
}

// --- Composition modules (deterministic loader, behavior-preserving) ---
// Walks CompositionRegistry in locked Order; identical to the prior
// Add*Composition sequence. See composition-loader.guard.md.
builder.Services.LoadModules(builder.Configuration);

// phase1-gate-api-edge: register the IExceptionHandler that maps
// ConcurrencyConflictException -> HTTP 409 + RFC 7807 ProblemDetails.
// This is the only seam where the H8b exception crosses the HTTP boundary.
builder.Services.AddExceptionHandler<ConcurrencyConflictExceptionHandler>();
// Maps DomainException -> HTTP 400 + RFC 7807. Domain invariant
// violations are a caller-correctable condition, not a server fault.
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
// phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): map transient OPA failures
// (timeout, transport, non-2xx, breaker-open) to 503 + Retry-After.
// Mirrors the ConcurrencyConflictExceptionHandler precedent — single
// edge seam, RETRYABLE REFUSAL response class, never an implicit allow.
builder.Services.AddExceptionHandler<PolicyEvaluationUnavailableExceptionHandler>();
// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): map outbox high-water-mark
// refusal to 503 + Retry-After. Same edge-handler precedent as the two
// preceding handlers; the typed exception bubbles untouched from
// PostgresOutboxAdapter to here, never converted to a silent drop.
builder.Services.AddExceptionHandler<OutboxSaturatedExceptionHandler>();
// phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): map workflow
// admission saturation to 503 + Retry-After. Same edge-handler
// precedent as the three preceding handlers; the typed exception
// bubbles untouched from WorkflowAdmissionGate to here, never
// converted to partial workflow execution.
builder.Services.AddExceptionHandler<WorkflowSaturatedExceptionHandler>();
// phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): map chain
// anchor commit-serializer wait timeout to 503 + Retry-After. Same
// edge-handler precedent as the four preceding handlers; the typed
// exception bubbles untouched from ChainAnchorService to here, never
// converted to an indefinite request hang.
builder.Services.AddExceptionHandler<ChainAnchorWaitTimeoutExceptionHandler>();
// phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): map chain-store
// transport failures and breaker-open refusals to 503 + Retry-After.
// Holder-side counterpart to the TC-2 wait-timeout handler. Same
// edge-handler precedent as the prior typed refusals.
builder.Services.AddExceptionHandler<ChainAnchorUnavailableExceptionHandler>();
// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): map workflow per-step
// / execution-level deadline expiry to 503 + Retry-After. Engine-side
// counterpart to WorkflowSaturatedExceptionHandler. Same edge-handler
// precedent as the prior typed refusals.
builder.Services.AddExceptionHandler<WorkflowTimeoutExceptionHandler>();
builder.Services.AddProblemDetails();

// phase1.5-S5.2.1 / PC-1 (INTAKE-CONFIG-01): declared admission control
// at the runtime HTTP edge. Closes R-01 (UNBOUNDED-OPEN intake) by
// installing a partitioned concurrency limiter sized from IntakeOptions.
// Partition key precedence: X-Tenant-Id header → remote IP. Overflow
// returns HTTP 429 with Retry-After (RETRYABLE REFUSAL). The OnRejected
// callback increments the Whycespace.Intake meter so saturation is visible.
var intakeOptions = new IntakeOptions
{
    GlobalConcurrency = builder.Configuration.GetValue<int?>("Intake:GlobalConcurrency")
        ?? new IntakeOptions().GlobalConcurrency,
    QueueLimit = builder.Configuration.GetValue<int?>("Intake:QueueLimit")
        ?? new IntakeOptions().QueueLimit,
    PerTenantConcurrency = builder.Configuration.GetValue<int?>("Intake:PerTenantConcurrency")
        ?? new IntakeOptions().PerTenantConcurrency,
    RejectionResponse = builder.Configuration.GetValue<string>("Intake:RejectionResponse")
        ?? new IntakeOptions().RejectionResponse,
    RetryAfterSeconds = builder.Configuration.GetValue<int?>("Intake:RetryAfterSeconds")
        ?? new IntakeOptions().RetryAfterSeconds,
};
builder.Services.AddSingleton(intakeOptions);

builder.Services.AddRateLimiter(options =>
{
    // Canonical refusal: 429 with Retry-After header. The body is left
    // empty (clients read the header); the existing exception-handler
    // chain is reserved for typed application exceptions and is not
    // reused for limiter rejections by design.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) =>
    {
        var partition = context.HttpContext.Request.Headers.ContainsKey("X-Tenant-Id")
            ? "tenant" : "ip";
        IntakeMetrics.Rejected.Add(1,
            new KeyValuePair<string, object?>("partition", partition),
            new KeyValuePair<string, object?>("path", context.HttpContext.Request.Path.Value ?? string.Empty));
        // QueueFull is a strict subset of Rejected: when QueueLimit is 0
        // every rejection is also a queue-full event; otherwise the
        // limiter rejects only after the queue itself is full, which is
        // semantically equivalent here under the partitioned-concurrency
        // shape (no separate "queue overflow vs concurrency overflow"
        // signal is exposed by the limiter).
        IntakeMetrics.QueueFull.Add(1,
            new KeyValuePair<string, object?>("partition", partition));
        context.HttpContext.Response.Headers.RetryAfter = intakeOptions.RetryAfterSeconds.ToString();
        return ValueTask.CompletedTask;
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Tenant partition takes precedence when present (the tenant
        // header is currently advisory — TodoController uses a fixed
        // "default" tenant — but the limiter is wired so the moment a
        // real tenant header arrives it acquires its own partition).
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantValues))
        {
            var tenant = tenantValues.ToString();
            if (!string.IsNullOrWhiteSpace(tenant))
            {
                return RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: $"tenant:{tenant}",
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = intakeOptions.PerTenantConcurrency,
                        QueueLimit = intakeOptions.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    });
            }
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: $"ip:{ip}",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = intakeOptions.GlobalConcurrency,
                QueueLimit = intakeOptions.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            });
    });
});

var app = builder.Build();

// HSID v2.1 H7 — fail-fast infrastructure gate. Verifies the hsid_sequences
// table exists before any HTTP traffic is accepted. Missing migration =
// host refuses to start. See deterministic-id.guard.md G19/G20.
using (var scope = app.Services.CreateScope())
{
    var validator = scope.ServiceProvider.GetRequiredService<HsidInfrastructureValidator>();
    await validator.ValidateAsync();
}

// HTTP observability middleware (Prometheus) — before routing.
// Lives in the host (not runtime) per R-TRACE-LAYER-DISCIPLINE-01:
// prometheus-net is an infra-layer dependency, not a runtime-layer one.
app.UseMiddleware<Whycespace.Platform.Host.Observability.HttpMetricsMiddleware>();
// Closed-loop correlation: stamp X-Correlation-Id on every request +
// echo on every response. Must run before MapControllers so the value is
// available to ControllerBase.RequestCorrelationId().
app.UseMiddleware<Whycespace.Platform.Api.Middleware.CorrelationIdMiddleware>();
app.UseRouting();
// R5.A / R-TRACE-CORRELATION-BRIDGE-01 — tags the current Activity with
// whyce.correlation_id and echoes the trace id back on `traceresponse`.
// Runs AFTER UseRouting so the AspNetCore auto-instrumentation root span
// is already in scope.
app.UseMiddleware<Whycespace.Platform.Api.Middleware.TraceCorrelationMiddleware>();
// R5.A Phase 2 / R-TRACE-LOG-CORRELATION-01 — wraps every request in an
// ILogger scope carrying trace_id / span_id / correlation_id / tenant_id
// so downstream log lines are trace-joinable. Runs AFTER both correlation
// middlewares so both ids are populated before the scope opens.
app.UseMiddleware<Whycespace.Platform.Api.Middleware.LogCorrelationMiddleware>();

// phase1.5-S5.2.1 / PC-1 (INTAKE-CONFIG-01): runtime intake admission
// limiter. MUST sit in front of MapControllers so every controller
// invocation is gated. After the limiter, a tiny inline middleware
// increments intake.admitted — by the time control reaches it, the
// limiter has either admitted or short-circuited via OnRejected.
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    var partition = context.Request.Headers.ContainsKey("X-Tenant-Id") ? "tenant" : "ip";
    IntakeMetrics.Admitted.Add(1,
        new KeyValuePair<string, object?>("partition", partition),
        new KeyValuePair<string, object?>("path", context.Request.Path.Value ?? string.Empty));
    await next();
});

// phase1.5-S5.2.3 / TC-9 (HOST-SHUTDOWN-DRAIN-01): link
// HttpContext.RequestAborted with IHostApplicationLifetime.ApplicationStopping
// so the controller-bound CancellationToken (TC-1) also fires when the
// host begins to drain. Must run BEFORE the exception handler and
// MapControllers so the linked token is in effect for the entire
// downstream pipeline. Sits AFTER the rate limiter so a shutdown does
// not perturb intake refusal accounting.
app.UseMiddleware<Whycespace.Platform.Api.Middleware.HostShutdownLinkingMiddleware>();

// phase1-gate-api-edge: invoke the registered IExceptionHandler chain
// (currently just ConcurrencyConflictExceptionHandler -> 409). Must be
// before MapControllers so it sits in front of the controller pipeline.
app.UseExceptionHandler();

// WP-1 (Security Binding Completion): HTTP authentication + authorization
// inserted after exception handler (so typed auth failures are caught) and
// before MapControllers (so [Authorize] attributes are enforced). Fail-closed:
// requests without valid JWT Bearer token receive 401 before reaching any
// controller marked [Authorize]. Health/liveness/readiness endpoints use
// [AllowAnonymous] and are exempt.
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Whycespace";
    options.SwaggerEndpoint("/swagger/operational/swagger.json", "Operational API");
    options.SwaggerEndpoint("/swagger/infrastructure/swagger.json", "Infrastructure API");
});

app.MapControllers();

// Prometheus /metrics endpoint
app.MapMetrics();

app.Run();
