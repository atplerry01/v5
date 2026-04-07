using Prometheus;
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

var app = builder.Build();

// HTTP observability middleware (Prometheus) — before routing
app.UseMiddleware<Whyce.Runtime.Observability.HttpMetricsMiddleware>();
app.UseRouting();

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
