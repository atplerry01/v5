using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Access.Session;
using Whycespace.Projections.Trust.Identity.Consent;
using Whycespace.Projections.Trust.Identity.Credential;
using Whycespace.Projections.Trust.Identity.Profile;
using Whycespace.Projections.Trust.Identity.Registry;
using Whycespace.Projections.Trust.Identity.Verification;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Trust.Access.Session;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Trust.Projection;

public static class TrustProjectionModule
{
    public static IServiceCollection AddTrustProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Projection stores ────────────────────────────────────
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<RegistryReadModel>("projection_trust_identity_registry", "registry_read_model", "Registry"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ProfileReadModel>("projection_trust_identity_profile", "profile_read_model", "Profile"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConsentReadModel>("projection_trust_identity_consent", "consent_read_model", "Consent"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SessionReadModel>("projection_trust_access_session", "session_read_model", "Session"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<CredentialReadModel>("projection_trust_identity_credential", "credential_read_model", "Credential"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<VerificationReadModel>("projection_trust_identity_verification", "verification_read_model", "Verification"));

        // ── Projection handlers ──────────────────────────────────
        services.AddSingleton(sp => new RegistryProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RegistryReadModel>>()));
        services.AddSingleton(sp => new ProfileProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ProfileReadModel>>()));
        services.AddSingleton(sp => new ConsentProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConsentReadModel>>()));
        services.AddSingleton(sp => new SessionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SessionReadModel>>()));
        services.AddSingleton(sp => new CredentialProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CredentialReadModel>>()));
        services.AddSingleton(sp => new VerificationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<VerificationReadModel>>()));

        // ── Kafka projection consumers (one per topic) ───────────
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.identity.registry.events",
            consumerGroup: "whyce.projection.trust.identity.registry",
            projectionSchema: "projection_trust_identity_registry",
            projectionTable: "registry_read_model",
            aggregateType: "Registry");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.identity.profile.events",
            consumerGroup: "whyce.projection.trust.identity.profile",
            projectionSchema: "projection_trust_identity_profile",
            projectionTable: "profile_read_model",
            aggregateType: "Profile");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.identity.consent.events",
            consumerGroup: "whyce.projection.trust.identity.consent",
            projectionSchema: "projection_trust_identity_consent",
            projectionTable: "consent_read_model",
            aggregateType: "Consent");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.access.session.events",
            consumerGroup: "whyce.projection.trust.access.session",
            projectionSchema: "projection_trust_access_session",
            projectionTable: "session_read_model",
            aggregateType: "Session");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.identity.credential.events",
            consumerGroup: "whyce.projection.trust.identity.credential",
            projectionSchema: "projection_trust_identity_credential",
            projectionTable: "credential_read_model",
            aggregateType: "Credential");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.trust.identity.verification.events",
            consumerGroup: "whyce.projection.trust.identity.verification",
            projectionSchema: "projection_trust_identity_verification",
            projectionTable: "verification_read_model",
            aggregateType: "Verification");

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var registryHandler = provider.GetRequiredService<RegistryProjectionHandler>();
        projection.Register("RegistrationInitiatedEvent", registryHandler);
        projection.Register("RegistrationVerifiedEvent", registryHandler);
        projection.Register("RegistrationActivatedEvent", registryHandler);
        projection.Register("RegistrationRejectedEvent", registryHandler);
        projection.Register("RegistrationLockedEvent", registryHandler);

        var profileHandler = provider.GetRequiredService<ProfileProjectionHandler>();
        projection.Register("ProfileCreatedEvent", profileHandler);
        projection.Register("ProfileActivatedEvent", profileHandler);
        projection.Register("ProfileDeactivatedEvent", profileHandler);

        var consentHandler = provider.GetRequiredService<ConsentProjectionHandler>();
        projection.Register("ConsentGrantedEvent", consentHandler);
        projection.Register("ConsentRevokedEvent", consentHandler);
        projection.Register("ConsentExpiredEvent", consentHandler);

        var sessionHandler = provider.GetRequiredService<SessionProjectionHandler>();
        projection.Register("SessionOpenedEvent", sessionHandler);
        projection.Register("SessionExpiredEvent", sessionHandler);
        projection.Register("SessionTerminatedEvent", sessionHandler);

        var credentialHandler = provider.GetRequiredService<CredentialProjectionHandler>();
        projection.Register("CredentialIssuedEvent", credentialHandler);
        projection.Register("CredentialActivatedEvent", credentialHandler);
        projection.Register("CredentialRevokedEvent", credentialHandler);

        var verificationHandler = provider.GetRequiredService<VerificationProjectionHandler>();
        projection.Register("VerificationInitiatedEvent", verificationHandler);
        projection.Register("VerificationPassedEvent", verificationHandler);
        projection.Register("VerificationFailedEvent", verificationHandler);
    }

    private static void RegisterWorker(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        string projectionSchema,
        string projectionTable,
        string aggregateType)
    {
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new GenericKafkaProjectionConsumerWorker(
                kafkaBootstrapServers,
                topic,
                consumerGroup,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ProjectionRegistry>(),
                new PostgresProjectionWriter(
                    sp.GetRequiredService<ProjectionsDataSource>(),
                    projectionSchema,
                    projectionTable,
                    aggregateType),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>(),
                pollTimeout: null,
                deadLetterStore: sp.GetService<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(),
                kafkaBreaker: sp.GetService<Whycespace.Shared.Contracts.Runtime.ICircuitBreakerRegistry>()?.TryGet("kafka-producer"),
                topicNameResolver: sp.GetService<TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<IRandomProvider>()));
    }
}
