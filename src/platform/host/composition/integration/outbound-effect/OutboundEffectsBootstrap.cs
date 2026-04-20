using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Projections.Integration.OutboundEffect;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.OutboundEffects;
using Whycespace.Runtime.Projection;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — bootstrap module for <c>integration-system/outbound-effect</c>.
/// Wires the dispatcher, relay, hosted-service shell, in-memory stores, adapter
/// registry, options registry, and schema module. No real providers register
/// here — R3.B.2 lands the first production adapter.
///
/// <para>The <see cref="NoOpOutboundEffectAdapter"/> is NOT registered by this
/// module (R-OUT-EFF-PROHIBITION-05). Tests register it in their own
/// composition root.</para>
/// </summary>
public sealed class OutboundEffectsBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<OutboundEffectLifecycleEventFactory>();

        // D-R3B-4 / R3.B.2: Postgres-backed queue store is the production
        // default. Tests and dev hosts that set OutboundEffects:QueueStore
        // to "InMemory" (or run without an EventStoreDataSource) fall back
        // to the in-memory reference implementation.
        var queueStoreMode = configuration["OutboundEffects:QueueStore"] ?? "Postgres";
        if (string.Equals(queueStoreMode, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IOutboundEffectQueueStore, InMemoryOutboundEffectQueueStore>();
        }
        else
        {
            services.AddSingleton<IOutboundEffectQueueStore>(sp =>
            {
                var dataSource = sp.GetService<EventStoreDataSource>();
                if (dataSource is null)
                {
                    // EventStoreDataSource not registered (test host without
                    // Postgres) — transparently fall back to in-memory.
                    return new InMemoryOutboundEffectQueueStore();
                }
                return new PostgresOutboundEffectQueueStore(
                    dataSource,
                    sp.GetRequiredService<IPayloadTypeRegistry>(),
                    sp.GetService<Microsoft.Extensions.Logging.ILogger<PostgresOutboundEffectQueueStore>>());
            });
        }

        services.AddSingleton<IOutboundEffectProjectionStore, InMemoryOutboundEffectProjectionStore>();
        services.AddSingleton<OutboundEffectProjectionHandler>();

        services.AddSingleton<IOutboundEffectAdapterRegistry>(sp =>
            new OutboundEffectAdapterRegistry(sp.GetServices<IOutboundEffectAdapter>()));

        services.AddSingleton<IOutboundEffectOptionsRegistry>(sp =>
            new OutboundEffectOptionsRegistry(sp.GetServices<OutboundEffectOptions>()));

        services.AddSingleton<OutboundEffectsMeter>();

        services.AddSingleton<OutboundEffectRelayOptions>(_ => new OutboundEffectRelayOptions
        {
            HostId = configuration["OutboundEffects:Relay:HostId"] ?? Environment.MachineName,
            BatchSize = int.TryParse(configuration["OutboundEffects:Relay:BatchSize"], out var b) ? b : 32,
            PollIntervalMs = int.TryParse(configuration["OutboundEffects:Relay:PollIntervalMs"], out var p) ? p : 500,
        });

        services.AddSingleton<OutboundEffectDispatcher>();
        services.AddSingleton<IOutboundEffectDispatcher>(sp => sp.GetRequiredService<OutboundEffectDispatcher>());
        services.AddSingleton<OutboundEffectRelay>();

        services.AddSingleton<IHostedService, OutboundEffectRelayWorker>();

        // R3.B.4 — finality service + reconciliation sweeper + webhook ingress.
        services.AddSingleton<OutboundEffectFinalityService>();
        services.AddSingleton<IOutboundEffectFinalityService>(sp =>
            sp.GetRequiredService<OutboundEffectFinalityService>());
        services.AddSingleton<OutboundEffectReconciliationSweeper>();
        services.AddSingleton<IHostedService, OutboundEffectReconciliationSweeperWorker>();

        services.AddSingleton<WebhookDeliverySignatureVerifier>(sp =>
        {
            var options = sp.GetServices<WebhookDeliveryOptions>().ToList();
            var keys = options.ToDictionary(o => o.ProviderId, o => o.SigningKey, StringComparer.Ordinal);
            return new WebhookDeliverySignatureVerifier(keys);
        });
        services.AddSingleton<WebhookCallbackIngressHandler>();

        // R3.B.5 — compensation signaling: registry populated from DI
        // (IEnumerable<IOutboundEffectCompensationHandler>), dispatcher fans
        // out signals to registered handlers with loud evidence on orphans.
        services.AddSingleton<IOutboundEffectCompensationHandlerRegistry>(sp =>
            new OutboundEffectCompensationHandlerRegistry(
                sp.GetServices<IOutboundEffectCompensationHandler>()));
        services.AddSingleton<OutboundEffectCompensationDispatcher>();

        // R3.B.2 — HTTP webhook delivery adapter (first real provider).
        RegisterWebhookDeliveryAdapter(services, configuration);
    }

    private static void RegisterWebhookDeliveryAdapter(IServiceCollection services, IConfiguration configuration)
    {
        const string ProviderId = "http-webhook";

        var webhookOptions = new WebhookDeliveryOptions
        {
            ProviderId = ProviderId,
            SigningKey = configuration["OutboundEffects:WebhookDelivery:SigningKey"] ?? string.Empty,
            SignatureHeader = configuration["OutboundEffects:WebhookDelivery:SignatureHeader"]
                ?? new WebhookDeliveryOptions { SigningKey = string.Empty }.SignatureHeader,
            EffectIdHeader = configuration["OutboundEffects:WebhookDelivery:EffectIdHeader"]
                ?? new WebhookDeliveryOptions { SigningKey = string.Empty }.EffectIdHeader,
        };
        services.AddSingleton(webhookOptions);

        // Per-provider OutboundEffectOptions. Defaults selected for a
        // real webhook delivery path: 5s per-attempt, 5m total budget,
        // 5 attempts, 2m ack timeout, 10m finality window.
        var outboundOptions = new OutboundEffectOptions
        {
            ProviderId = ProviderId,
            DispatchTimeoutMs = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:DispatchTimeoutMs"], out var dt)
                ? dt : 5_000,
            TotalBudgetMs = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:TotalBudgetMs"], out var tb)
                ? tb : 300_000,
            AckTimeoutMs = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:AckTimeoutMs"], out var at)
                ? at : 120_000,
            FinalityWindowMs = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:FinalityWindowMs"], out var fw)
                ? fw : 600_000,
            MaxAttempts = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:MaxAttempts"], out var ma)
                ? ma : 5,
            BreakerFailureThreshold = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:BreakerThreshold"], out var bt)
                ? bt : 5,
            BreakerWindowSeconds = int.TryParse(
                configuration["OutboundEffects:WebhookDelivery:BreakerWindowSeconds"], out var bw)
                ? bw : 30,
        };
        services.AddSingleton(outboundOptions);

        // R3.B.2 / parent design §6.2 — per-provider ICircuitBreaker named
        // "outbound.{providerId}". Registered as an enumerable contributor so
        // CircuitBreakerRegistry picks it up via sp.GetServices<ICircuitBreaker>().
        services.AddSingleton<ICircuitBreaker>(sp =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions
                {
                    Name = OutboundEffectRelay.BreakerNamePrefix + ProviderId,
                    FailureThreshold = outboundOptions.BreakerFailureThreshold,
                    WindowSeconds = outboundOptions.BreakerWindowSeconds,
                },
                sp.GetRequiredService<IClock>()));

        // HttpClient for the webhook adapter. Timeout is left at the .NET
        // default; the relay enforces per-attempt timeout via linked CTS.
        services.AddSingleton<WebhookDeliveryAdapter>(sp =>
            new WebhookDeliveryAdapter(
                new HttpClient(),
                sp.GetRequiredService<WebhookDeliveryOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<WebhookDeliveryAdapter>>()));
        services.AddSingleton<IOutboundEffectAdapter>(sp =>
            sp.GetRequiredService<WebhookDeliveryAdapter>());
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterIntegrationOutboundEffect(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<OutboundEffectProjectionHandler>();
        projection.Register("OutboundEffectScheduledEvent", handler);
        projection.Register("OutboundEffectDispatchedEvent", handler);
        projection.Register("OutboundEffectAcknowledgedEvent", handler);
        projection.Register("OutboundEffectDispatchFailedEvent", handler);
        projection.Register("OutboundEffectRetryAttemptedEvent", handler);
        projection.Register("OutboundEffectRetryExhaustedEvent", handler);
        projection.Register("OutboundEffectFinalizedEvent", handler);
        projection.Register("OutboundEffectReconciliationRequiredEvent", handler);
        projection.Register("OutboundEffectReconciledEvent", handler);
        projection.Register("OutboundEffectCompensationRequestedEvent", handler);
        projection.Register("OutboundEffectCancelledEvent", handler);
    }

    public void RegisterEngines(IEngineRegistry engine) { }
    public void RegisterWorkflows(IWorkflowRegistry workflow) { }

    public void RegisterPayloadTypes(IPayloadTypeRegistry registry)
    {
        registry.Register<WebhookEffectPayload>();
    }
}
