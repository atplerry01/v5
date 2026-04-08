using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Whyce.Platform.Api.Middleware;
using Whyce.Platform.Host.Adapters;
using Whyce.Platform.Host.Bootstrap;
using Whyce.Platform.Host.Composition;
using Whyce.Platform.Host.Composition.Loader;
using Whyce.Shared.Contracts.Infrastructure.Admission;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddProblemDetails();

// phase1.5-S5.2.1 / PC-1 (INTAKE-CONFIG-01): declared admission control
// at the runtime HTTP edge. Closes R-01 (UNBOUNDED-OPEN intake) by
// installing a partitioned concurrency limiter sized from IntakeOptions.
// Partition key precedence: X-Tenant-Id header → remote IP. Overflow
// returns HTTP 429 with Retry-After (RETRYABLE REFUSAL). The OnRejected
// callback increments the Whyce.Intake meter so saturation is visible.
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

// HTTP observability middleware (Prometheus) — before routing
app.UseMiddleware<Whyce.Runtime.Observability.HttpMetricsMiddleware>();
app.UseRouting();

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

// phase1-gate-api-edge: invoke the registered IExceptionHandler chain
// (currently just ConcurrencyConflictExceptionHandler -> 409). Must be
// before MapControllers so it sits in front of the controller pipeline.
app.UseExceptionHandler();

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
