using Microsoft.Extensions.DependencyInjection;

namespace Whyce.Platform.Host.Composition.Infrastructure.Observability;

/// <summary>
/// Infrastructure-level observability capability — reserved for future
/// infrastructure-scoped metrics exporters, tracing collectors, and
/// structured logging configuration. Health checks and the runtime
/// state aggregator remain in the top-level ObservabilityComposition
/// module (Order 4); this module covers infrastructure adapter
/// instrumentation that sits below that layer.
/// </summary>
public static class ObservabilityInfrastructureModule
{
    public static IServiceCollection AddInfrastructureObservability(this IServiceCollection services)
    {
        // No registrations yet. Future candidates:
        // - OpenTelemetry TracerProvider for Postgres/Kafka/Redis spans
        // - Prometheus meter bindings for infrastructure adapter internals
        // - Structured logging enrichment for infrastructure correlation IDs
        return services;
    }
}
