using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Whycespace.Runtime.Observability;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Observability;

/// <summary>
/// R5.A / R-TRACE-EXPORTER-OTEL-01 — OpenTelemetry tracing bootstrap.
/// Registers the canonical <see cref="WhyceActivitySources"/> with
/// <see cref="TracerProvider"/>, wires AspNetCore + Http auto-instrumentation,
/// and configures the OTLP exporter. Deterministic, fail-closed defaults:
///
/// <list type="bullet">
///   <item>Service name <c>whyce-runtime</c> — overridable via
///         <c>Otel:ServiceName</c>.</item>
///   <item>OTLP endpoint default <c>http://jaeger:4317</c> — matches the
///         Jaeger all-in-one service in <c>infrastructure/deployment/docker-compose.yml</c>.
///         Override via <c>Otel:Endpoint</c>.</item>
///   <item>AlwaysOn sampler — R5.A Phase 1 does not tune sampling; per-env
///         head-based sampling is R5.A Phase 2 / R5.C.</item>
/// </list>
///
/// <para>Layer discipline: <c>Whycespace.Runtime</c> carries NO OpenTelemetry
/// NuGet reference. Runtime code emits spans via the
/// <see cref="System.Diagnostics.ActivitySource"/> stdlib; this module is
/// the sole seam where OTEL lights up the exporter.</para>
/// </summary>
public static class TracingInfrastructureModule
{
    public const string ServiceNameDefault = "whyce-runtime";
    public const string OtlpEndpointDefault = "http://jaeger:4317";

    public static IServiceCollection AddTracing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration.GetValue<string>("Otel:ServiceName") ?? ServiceNameDefault;
        var endpoint = configuration.GetValue<string>("Otel:Endpoint") ?? OtlpEndpointDefault;

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName, serviceVersion: "1.0.0"));

                // Runtime-layer canonical sources — spans from
                // SystemIntentDispatcher (command.dispatch),
                // OperatorActionAuditRecorder (admin.operator_action),
                // EventFabric.Process{,Audit}Async (event.fabric.*),
                // and OutboundEffectDispatcher.ScheduleAsync (outbound.effect.schedule).
                tracing.AddSource(WhyceActivitySources.ControlPlaneName);
                tracing.AddSource(WhyceActivitySources.AdminName);
                tracing.AddSource(WhyceActivitySources.EventFabricName);
                tracing.AddSource(WhyceActivitySources.OutboundEffectsName);

                // HTTP edge auto-instrumentation. These light up every
                // controller call + outbound HttpClient call; our canonical
                // runtime spans become child spans of the AspNetCore root.
                tracing.AddAspNetCoreInstrumentation(opts =>
                {
                    // Suppress the /metrics scrape noise — Prometheus polls
                    // every 5s and its spans drown out useful traffic.
                    opts.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/metrics") &&
                        !ctx.Request.Path.StartsWithSegments("/live") &&
                        !ctx.Request.Path.StartsWithSegments("/ready");
                });
                tracing.AddHttpClientInstrumentation();

                tracing.AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(endpoint);
                });
            });

        return services;
    }
}
