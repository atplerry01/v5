using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Whyce.Platform.Api.Middleware;
using Whyce.Platform.Host.Bootstrap;
using Whyce.Platform.Host.Composition;
using Whyce.Platform.Host.Composition.Loader;

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
builder.Services.AddProblemDetails();

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
