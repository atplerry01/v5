using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Bootstrap;
using Whycespace.Platform.Host.Composition.Trust.Access.Session;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Security;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust;
using Whycespace.Platform.Host.Composition.Trust.Identity.Consent;
using Whycespace.Platform.Host.Composition.Trust.Identity.Credential;
using Whycespace.Platform.Host.Composition.Trust.Identity.Profile;
using Whycespace.Platform.Host.Composition.Trust.Identity.Registry;
using Whycespace.Platform.Host.Composition.Trust.Identity.Verification;
using Whycespace.Platform.Host.Composition.Trust.Identity.Registry.Workflow;
using Whycespace.Platform.Host.Composition.Trust.Projection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Trust;

/// <summary>
/// Phase 2.8 — bootstrap module for the trust-system.
/// Registers event schema bindings for all 17 trust-system aggregates across the
/// identity context (11 BCs) and access context (6 BCs).
///
/// Kafka topics resolve automatically from the canonical formula:
///   whyce.trust.{context}.{domain}.{type}
/// e.g. whyce.trust.identity.consent.events, whyce.trust.access.grant.events
///
/// Projections (profile, consent, session) are registered via TrustProjectionModule.
/// Engine registrations are deferred to a later batch.
/// </summary>
public sealed class TrustSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRegistryApplication();
        services.AddProfileApplication();
        services.AddConsentApplication();
        services.AddSessionApplication();
        services.AddCredentialApplication();
        services.AddVerificationApplication();
        services.AddTrustProjection(configuration);

        services.AddTrustPolicyBindings();
        services.AddSingleton<IIdentityThrottlePolicy, InMemoryIdentityThrottlePolicy>();
        services.AddSingleton<ITrustMetrics, TrustIdentityMetrics>();

        services.AddRegistrationOnboardingWorkflow();

        var operatorEmail = configuration["Bootstrap:FirstOperator:Email"]
            ?? throw new InvalidOperationException(
                "Bootstrap:FirstOperator:Email is required. " +
                "Configure the first-operator email in application settings before starting the host.");
        var operatorType = configuration["Bootstrap:FirstOperator:RegistrationType"]
            ?? throw new InvalidOperationException(
                "Bootstrap:FirstOperator:RegistrationType is required. " +
                "Configure the first-operator registration type in application settings before starting the host.");

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new PlatformBootstrapService(
                sp.GetRequiredService<ISystemIntentDispatcher>(),
                sp.GetRequiredService<Whycespace.Shared.Kernel.Domain.IIdGenerator>(),
                sp.GetRequiredService<Whycespace.Shared.Kernel.Domain.IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Persistence.IEventStore>(),
                operatorEmail,
                operatorType,
                sp.GetService<Microsoft.Extensions.Logging.ILogger<PlatformBootstrapService>>()));

        services.AddSingleton<RegistrationOnboardingTriggerHandler>();
        services.AddSingleton<RegistrationActivatedCrossDomainHandler>();

        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new RegistrationOnboardingTriggerWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<RegistrationOnboardingTriggerHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<RegistrationOnboardingTriggerWorker>>(),
                producer: sp.GetRequiredService<Confluent.Kafka.IProducer<string, string>>(),
                topicNameResolver: sp.GetService<Whycespace.Runtime.EventFabric.TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<IRandomProvider>(),
                kafkaBreaker: sp.GetService<Whycespace.Shared.Contracts.Runtime.ICircuitBreakerRegistry>()?.TryGet("kafka-producer")));

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new RegistrationActivatedCrossDomainWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<RegistrationActivatedCrossDomainHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<RegistrationActivatedCrossDomainWorker>>(),
                producer: sp.GetRequiredService<Confluent.Kafka.IProducer<string, string>>(),
                topicNameResolver: sp.GetService<Whycespace.Runtime.EventFabric.TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<IRandomProvider>(),
                kafkaBreaker: sp.GetService<Whycespace.Shared.Contracts.Runtime.ICircuitBreakerRegistry>()?.TryGet("kafka-producer")));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        // Identity context — 11 BCs
        DomainSchemaCatalog.RegisterTrustIdentityIdentity(schema);
        DomainSchemaCatalog.RegisterTrustIdentityCredential(schema);
        DomainSchemaCatalog.RegisterTrustIdentityVerification(schema);
        DomainSchemaCatalog.RegisterTrustIdentityProfile(schema);
        DomainSchemaCatalog.RegisterTrustIdentityRegistry(schema);
        DomainSchemaCatalog.RegisterTrustIdentityTrust(schema);
        DomainSchemaCatalog.RegisterTrustIdentityServiceIdentity(schema);
        DomainSchemaCatalog.RegisterTrustIdentityConsent(schema);
        DomainSchemaCatalog.RegisterTrustIdentityDevice(schema);
        DomainSchemaCatalog.RegisterTrustIdentityIdentityGraph(schema);
        DomainSchemaCatalog.RegisterTrustIdentityFederation(schema);

        // Access context — 6 BCs
        DomainSchemaCatalog.RegisterTrustAccessSession(schema);
        DomainSchemaCatalog.RegisterTrustAccessRole(schema);
        DomainSchemaCatalog.RegisterTrustAccessGrant(schema);
        DomainSchemaCatalog.RegisterTrustAccessRequest(schema);
        DomainSchemaCatalog.RegisterTrustAccessPermission(schema);
        DomainSchemaCatalog.RegisterTrustAccessAuthorization(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        TrustProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        RegistryApplicationModule.RegisterEngines(engine);
        ProfileApplicationModule.RegisterEngines(engine);
        ConsentApplicationModule.RegisterEngines(engine);
        SessionApplicationModule.RegisterEngines(engine);
        CredentialApplicationModule.RegisterEngines(engine);
        VerificationApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        RegistrationOnboardingWorkflowModule.RegisterWorkflows(workflow);
    }
}
