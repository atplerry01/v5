using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Projections.Business.Agreement.ChangeControl.Approval;
using Whycespace.Projections.Business.Agreement.ChangeControl.Clause;
using Whycespace.Projections.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Projections.Business.Agreement.Commitment.Acceptance;
using Whycespace.Projections.Business.Agreement.Commitment.Contract;
using Whycespace.Projections.Business.Agreement.Commitment.Obligation;
using Whycespace.Projections.Business.Agreement.Commitment.Validity;
using Whycespace.Projections.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Projections.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Projections.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Projections.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Projections.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Projections.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Projections.Business.Entitlement.UsageControl.Limit;
using Whycespace.Projections.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Projections.Business.Order.OrderChange.Cancellation;
using Whycespace.Projections.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Projections.Business.Order.OrderCore.LineItem;
using Whycespace.Projections.Business.Order.OrderCore.Reservation;
using OrderAmendmentProj = Whycespace.Projections.Business.Order.OrderChange.Amendment;
using OrderCoreOrderProj = Whycespace.Projections.Business.Order.OrderCore.Order;
using Whycespace.Projections.Business.Service.ServiceConstraint.PolicyBinding;
using ServiceConstraintProj = Whycespace.Projections.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Projections.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Projections.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Projections.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Projections.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Projections.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Projections.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Projections.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Projections.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Projections.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Projections.Business.Offering.CatalogCore.Bundle;
using OfferingCatalogProj = Whycespace.Projections.Business.Offering.CatalogCore.Catalog;
using Whycespace.Projections.Business.Offering.CatalogCore.Product;
using Whycespace.Projections.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Projections.Business.Offering.CommercialShape.Configuration;
using Whycespace.Projections.Business.Offering.CommercialShape.Package;
using Whycespace.Projections.Business.Offering.CommercialShape.Plan;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using OrderAmendmentRm = Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using OrderCoreOrderRm = Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using ServiceConstraintRm = Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using OfferingCatalogRm = Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Business.Projection;

/// <summary>
/// Business-system projection module. Covers the full agreement context —
/// 10 BCs × (projection store + handler + Kafka consumer worker).
/// </summary>
public static class BusinessProjectionModule
{
    public static IServiceCollection AddBusinessProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        // Commitment group
        WireBc<ContractReadModel, ContractProjectionHandler>(
            services, kafkaBootstrapServers, "contract");
        WireBc<AcceptanceReadModel, AcceptanceProjectionHandler>(
            services, kafkaBootstrapServers, "acceptance");
        WireBc<ObligationReadModel, ObligationProjectionHandler>(
            services, kafkaBootstrapServers, "obligation");
        WireBc<ValidityReadModel, ValidityProjectionHandler>(
            services, kafkaBootstrapServers, "validity");

        // Change-control group
        WireBc<AmendmentReadModel, AmendmentProjectionHandler>(
            services, kafkaBootstrapServers, "amendment");
        WireBc<ApprovalReadModel, ApprovalProjectionHandler>(
            services, kafkaBootstrapServers, "approval");
        WireBc<ClauseReadModel, ClauseProjectionHandler>(
            services, kafkaBootstrapServers, "clause");
        WireBc<RenewalReadModel, RenewalProjectionHandler>(
            services, kafkaBootstrapServers, "renewal");

        // Party-governance group
        WireBc<CounterpartyReadModel, CounterpartyProjectionHandler>(
            services, kafkaBootstrapServers, "counterparty");
        WireBc<SignatureReadModel, SignatureProjectionHandler>(
            services, kafkaBootstrapServers, "signature");

        // Customer / identity-and-profile
        WireCustomerBc<AccountReadModel, AccountProjectionHandler>(
            services, kafkaBootstrapServers, "account");
        WireCustomerBc<CustomerReadModel, CustomerProjectionHandler>(
            services, kafkaBootstrapServers, "customer");
        WireCustomerBc<ProfileReadModel, ProfileProjectionHandler>(
            services, kafkaBootstrapServers, "profile");

        // Customer / segmentation-and-lifecycle
        WireCustomerBc<ContactPointReadModel, ContactPointProjectionHandler>(
            services, kafkaBootstrapServers, "contact_point", "whyce.business.customer.contact-point.events", "whyce.business.customer.contact-point");
        WireCustomerBc<LifecycleReadModel, LifecycleProjectionHandler>(
            services, kafkaBootstrapServers, "lifecycle");
        WireCustomerBc<SegmentReadModel, SegmentProjectionHandler>(
            services, kafkaBootstrapServers, "segment");

        // Entitlement / eligibility-and-grant
        WireEntitlementBc<AssignmentReadModel, AssignmentProjectionHandler>(
            services, kafkaBootstrapServers, "assignment");
        WireEntitlementBc<EligibilityReadModel, EligibilityProjectionHandler>(
            services, kafkaBootstrapServers, "eligibility");
        WireEntitlementBc<GrantReadModel, GrantProjectionHandler>(
            services, kafkaBootstrapServers, "grant");

        // Entitlement / usage-control
        WireEntitlementBc<AllocationReadModel, AllocationProjectionHandler>(
            services, kafkaBootstrapServers, "allocation");
        WireEntitlementBc<LimitReadModel, LimitProjectionHandler>(
            services, kafkaBootstrapServers, "limit");
        WireEntitlementBc<UsageRightReadModel, UsageRightProjectionHandler>(
            services, kafkaBootstrapServers, "usage_right", "whyce.business.entitlement.usage-right.events", "whyce.business.entitlement.usage-right");

        // Order / order-change
        WireOrderBc<OrderAmendmentRm.AmendmentReadModel, OrderAmendmentProj.AmendmentProjectionHandler>(
            services, kafkaBootstrapServers, "amendment");
        WireOrderBc<CancellationReadModel, CancellationProjectionHandler>(
            services, kafkaBootstrapServers, "cancellation");
        WireOrderBc<FulfillmentInstructionReadModel, FulfillmentInstructionProjectionHandler>(
            services, kafkaBootstrapServers, "fulfillment_instruction", "whyce.business.order.fulfillment-instruction.events", "whyce.business.order.fulfillment-instruction");

        // Order / order-core
        WireOrderBc<LineItemReadModel, LineItemProjectionHandler>(
            services, kafkaBootstrapServers, "line_item", "whyce.business.order.line-item.events", "whyce.business.order.line-item");
        WireOrderBc<OrderCoreOrderRm.OrderReadModel, OrderCoreOrderProj.OrderProjectionHandler>(
            services, kafkaBootstrapServers, "order");
        WireOrderBc<ReservationReadModel, ReservationProjectionHandler>(
            services, kafkaBootstrapServers, "reservation");

        // Service / service-constraint
        WireServiceBc<PolicyBindingReadModel, PolicyBindingProjectionHandler>(
            services, kafkaBootstrapServers, "policy_binding", "whyce.business.service.policy-binding.events", "whyce.business.service.policy-binding");
        WireServiceBc<ServiceConstraintRm.ServiceConstraintReadModel, ServiceConstraintProj.ServiceConstraintProjectionHandler>(
            services, kafkaBootstrapServers, "service_constraint", "whyce.business.service.service-constraint.events", "whyce.business.service.service-constraint");
        WireServiceBc<ServiceWindowReadModel, ServiceWindowProjectionHandler>(
            services, kafkaBootstrapServers, "service_window", "whyce.business.service.service-window.events", "whyce.business.service.service-window");

        // Service / service-core
        WireServiceBc<ServiceDefinitionReadModel, ServiceDefinitionProjectionHandler>(
            services, kafkaBootstrapServers, "service_definition", "whyce.business.service.service-definition.events", "whyce.business.service.service-definition");
        WireServiceBc<ServiceLevelReadModel, ServiceLevelProjectionHandler>(
            services, kafkaBootstrapServers, "service_level", "whyce.business.service.service-level.events", "whyce.business.service.service-level");
        WireServiceBc<ServiceOptionReadModel, ServiceOptionProjectionHandler>(
            services, kafkaBootstrapServers, "service_option", "whyce.business.service.service-option.events", "whyce.business.service.service-option");

        // Provider / provider-core + governance + scope
        WireProviderBc<ProviderCapabilityReadModel, ProviderCapabilityProjectionHandler>(
            services, kafkaBootstrapServers, "provider_capability", "whyce.business.provider.provider-capability.events", "whyce.business.provider.provider-capability");
        WireProviderBc<ProviderTierReadModel, ProviderTierProjectionHandler>(
            services, kafkaBootstrapServers, "provider_tier", "whyce.business.provider.provider-tier.events", "whyce.business.provider.provider-tier");
        WireProviderBc<ProviderAgreementReadModel, ProviderAgreementProjectionHandler>(
            services, kafkaBootstrapServers, "provider_agreement", "whyce.business.provider.provider-agreement.events", "whyce.business.provider.provider-agreement");
        WireProviderBc<ProviderAvailabilityReadModel, ProviderAvailabilityProjectionHandler>(
            services, kafkaBootstrapServers, "provider_availability", "whyce.business.provider.provider-availability.events", "whyce.business.provider.provider-availability");
        WireProviderBc<ProviderCoverageReadModel, ProviderCoverageProjectionHandler>(
            services, kafkaBootstrapServers, "provider_coverage", "whyce.business.provider.provider-coverage.events", "whyce.business.provider.provider-coverage");

        // Offering / catalog-core
        WireOfferingBc<BundleReadModel, BundleProjectionHandler>(
            services, kafkaBootstrapServers, "bundle");
        WireOfferingBc<OfferingCatalogRm.CatalogReadModel, OfferingCatalogProj.CatalogProjectionHandler>(
            services, kafkaBootstrapServers, "catalog");
        WireOfferingBc<ProductReadModel, ProductProjectionHandler>(
            services, kafkaBootstrapServers, "product");
        WireOfferingBc<ServiceOfferingReadModel, ServiceOfferingProjectionHandler>(
            services, kafkaBootstrapServers, "service_offering", "whyce.business.offering.service-offering.events", "whyce.business.offering.service-offering");

        // Offering / commercial-shape
        WireOfferingBc<ConfigurationReadModel, ConfigurationProjectionHandler>(
            services, kafkaBootstrapServers, "configuration");
        WireOfferingBc<PackageReadModel, PackageProjectionHandler>(
            services, kafkaBootstrapServers, "package");
        WireOfferingBc<PlanReadModel, PlanProjectionHandler>(
            services, kafkaBootstrapServers, "plan");

        return services;
    }

    private static void WireCustomerBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_customer_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.customer.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.customer.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireOfferingBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_offering_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.offering.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.offering.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireProviderBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_provider_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.provider.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.provider.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireServiceBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_service_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.service.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.service.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireOrderBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_order_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.order.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.order.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireEntitlementBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc,
        string? topicOverride = null,
        string? groupOverride = null)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema  = $"projection_business_entitlement_{bc}";
        var table   = $"{bc}_read_model";
        var aggType = PascalFromSnake(bc);
        var topic   = topicOverride ?? $"whyce.business.entitlement.{bc.Replace('_', '-')}.events";
        var group   = groupOverride ?? $"whyce.projection.business.entitlement.{bc.Replace('_', '-')}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static string PascalFromSnake(string s)
    {
        var parts = s.Split('_');
        return string.Concat(parts.Select(p => string.IsNullOrEmpty(p) ? p : char.ToUpperInvariant(p[0]) + p[1..]));
    }

    private static void WireBc<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string bc)
        where TReadModel : class, new()
        where THandler : class
    {
        var schema    = $"projection_business_agreement_{bc}";
        var table     = $"{bc}_read_model";
        var aggType   = char.ToUpperInvariant(bc[0]) + bc[1..];
        var topic     = $"whyce.business.agreement.{bc}.events";
        var group     = $"whyce.projection.business.agreement.{bc}";
        WireWorker<TReadModel, THandler>(services, kafkaBootstrapServers, schema, table, aggType, topic, group);
    }

    private static void WireWorker<TReadModel, THandler>(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string schema,
        string table,
        string aggType,
        string topic,
        string group)
        where TReadModel : class, new()
        where THandler : class
    {

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TReadModel>(schema, table, aggType));

        services.AddSingleton<THandler>();

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new GenericKafkaProjectionConsumerWorker(
                kafkaBootstrapServers,
                topic,
                group,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ProjectionRegistry>(),
                new PostgresProjectionWriter(
                    sp.GetRequiredService<ProjectionsDataSource>(),
                    schema,
                    table,
                    aggType),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>(),
                pollTimeout: null,
                deadLetterStore: sp.GetService<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(),
                kafkaBreaker: sp.GetService<Whycespace.Shared.Contracts.Runtime.ICircuitBreakerRegistry>()?.TryGet("kafka-producer"),
                topicNameResolver: sp.GetService<Whycespace.Runtime.EventFabric.TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<Whycespace.Shared.Kernel.Domain.IRandomProvider>()));
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        // ── Commitment ────────────────────────────────────────────
        var contractHandler = provider.GetRequiredService<ContractProjectionHandler>();
        projection.Register("ContractCreatedEvent",     contractHandler);
        projection.Register("ContractPartyAddedEvent",  contractHandler);
        projection.Register("ContractActivatedEvent",   contractHandler);
        projection.Register("ContractSuspendedEvent",   contractHandler);
        projection.Register("ContractTerminatedEvent",  contractHandler);

        var acceptanceHandler = provider.GetRequiredService<AcceptanceProjectionHandler>();
        projection.Register("AcceptanceCreatedEvent",  acceptanceHandler);
        projection.Register("AcceptanceAcceptedEvent", acceptanceHandler);
        projection.Register("AcceptanceRejectedEvent", acceptanceHandler);

        var obligationHandler = provider.GetRequiredService<ObligationProjectionHandler>();
        projection.Register("ObligationCreatedEvent",    obligationHandler);
        projection.Register("ObligationFulfilledEvent",  obligationHandler);
        projection.Register("ObligationBreachedEvent",   obligationHandler);

        var validityHandler = provider.GetRequiredService<ValidityProjectionHandler>();
        projection.Register("ValidityCreatedEvent",      validityHandler);
        projection.Register("ValidityExpiredEvent",      validityHandler);
        projection.Register("ValidityInvalidatedEvent",  validityHandler);

        // ── Change-control ────────────────────────────────────────
        var amendmentHandler = provider.GetRequiredService<AmendmentProjectionHandler>();
        projection.Register("AmendmentCreatedEvent",   amendmentHandler);
        projection.Register("AmendmentAppliedEvent",   amendmentHandler);
        projection.Register("AmendmentRevertedEvent",  amendmentHandler);

        var approvalHandler = provider.GetRequiredService<ApprovalProjectionHandler>();
        projection.Register("ApprovalCreatedEvent",   approvalHandler);
        projection.Register("ApprovalApprovedEvent",  approvalHandler);
        projection.Register("ApprovalRejectedEvent",  approvalHandler);

        var clauseHandler = provider.GetRequiredService<ClauseProjectionHandler>();
        projection.Register("ClauseCreatedEvent",      clauseHandler);
        projection.Register("ClauseActivatedEvent",    clauseHandler);
        projection.Register("ClauseSupersededEvent",   clauseHandler);

        var renewalHandler = provider.GetRequiredService<RenewalProjectionHandler>();
        projection.Register("RenewalCreatedEvent",  renewalHandler);
        projection.Register("RenewalRenewedEvent",  renewalHandler);
        projection.Register("RenewalExpiredEvent",  renewalHandler);

        // ── Party-governance ──────────────────────────────────────
        var counterpartyHandler = provider.GetRequiredService<CounterpartyProjectionHandler>();
        projection.Register("CounterpartyCreatedEvent",     counterpartyHandler);
        projection.Register("CounterpartySuspendedEvent",   counterpartyHandler);
        projection.Register("CounterpartyTerminatedEvent",  counterpartyHandler);

        var signatureHandler = provider.GetRequiredService<SignatureProjectionHandler>();
        projection.Register("SignatureCreatedEvent",  signatureHandler);
        projection.Register("SignatureSignedEvent",   signatureHandler);
        projection.Register("SignatureRevokedEvent",  signatureHandler);

        // ── Customer / identity-and-profile ───────────────────────
        var accountHandler = provider.GetRequiredService<AccountProjectionHandler>();
        projection.Register("AccountCreatedEvent",    accountHandler);
        projection.Register("AccountRenamedEvent",    accountHandler);
        projection.Register("AccountActivatedEvent",  accountHandler);
        projection.Register("AccountSuspendedEvent",  accountHandler);
        projection.Register("AccountClosedEvent",     accountHandler);

        var customerHandler = provider.GetRequiredService<CustomerProjectionHandler>();
        projection.Register("CustomerCreatedEvent",      customerHandler);
        projection.Register("CustomerRenamedEvent",      customerHandler);
        projection.Register("CustomerReclassifiedEvent", customerHandler);
        projection.Register("CustomerActivatedEvent",    customerHandler);
        projection.Register("CustomerArchivedEvent",     customerHandler);

        var profileHandler = provider.GetRequiredService<ProfileProjectionHandler>();
        projection.Register("ProfileCreatedEvent",           profileHandler);
        projection.Register("ProfileRenamedEvent",           profileHandler);
        projection.Register("ProfileDescriptorSetEvent",     profileHandler);
        projection.Register("ProfileDescriptorRemovedEvent", profileHandler);
        projection.Register("ProfileActivatedEvent",         profileHandler);
        projection.Register("ProfileArchivedEvent",          profileHandler);

        // ── Customer / segmentation-and-lifecycle ─────────────────
        var contactPointHandler = provider.GetRequiredService<ContactPointProjectionHandler>();
        projection.Register("ContactPointCreatedEvent",       contactPointHandler);
        projection.Register("ContactPointUpdatedEvent",       contactPointHandler);
        projection.Register("ContactPointActivatedEvent",     contactPointHandler);
        projection.Register("ContactPointPreferredSetEvent",  contactPointHandler);
        projection.Register("ContactPointArchivedEvent",      contactPointHandler);

        var lifecycleHandler = provider.GetRequiredService<LifecycleProjectionHandler>();
        projection.Register("LifecycleStartedEvent",      lifecycleHandler);
        projection.Register("LifecycleStageChangedEvent", lifecycleHandler);
        projection.Register("LifecycleClosedEvent",       lifecycleHandler);

        var segmentHandler = provider.GetRequiredService<SegmentProjectionHandler>();
        projection.Register("SegmentCreatedEvent",   segmentHandler);
        projection.Register("SegmentUpdatedEvent",   segmentHandler);
        projection.Register("SegmentActivatedEvent", segmentHandler);
        projection.Register("SegmentArchivedEvent",  segmentHandler);

        // ── Entitlement / eligibility-and-grant ───────────────────
        var assignmentHandler = provider.GetRequiredService<AssignmentProjectionHandler>();
        projection.Register("AssignmentCreatedEvent",   assignmentHandler);
        projection.Register("AssignmentActivatedEvent", assignmentHandler);
        projection.Register("AssignmentRevokedEvent",   assignmentHandler);

        var eligibilityHandler = provider.GetRequiredService<EligibilityProjectionHandler>();
        projection.Register("EligibilityCreatedEvent",             eligibilityHandler);
        projection.Register("EligibilityEvaluatedEligibleEvent",   eligibilityHandler);
        projection.Register("EligibilityEvaluatedIneligibleEvent", eligibilityHandler);

        var grantHandler = provider.GetRequiredService<GrantProjectionHandler>();
        projection.Register("GrantCreatedEvent",   grantHandler);
        projection.Register("GrantActivatedEvent", grantHandler);
        projection.Register("GrantRevokedEvent",   grantHandler);
        projection.Register("GrantExpiredEvent",   grantHandler);

        // ── Entitlement / usage-control ───────────────────────────
        var allocationHandler = provider.GetRequiredService<AllocationProjectionHandler>();
        projection.Register("AllocationCreatedEvent",   allocationHandler);
        projection.Register("AllocationAllocatedEvent", allocationHandler);
        projection.Register("AllocationReleasedEvent",  allocationHandler);

        var limitHandler = provider.GetRequiredService<LimitProjectionHandler>();
        projection.Register("LimitCreatedEvent",  limitHandler);
        projection.Register("LimitEnforcedEvent", limitHandler);
        projection.Register("LimitBreachedEvent", limitHandler);

        var usageRightHandler = provider.GetRequiredService<UsageRightProjectionHandler>();
        projection.Register("UsageRightCreatedEvent",  usageRightHandler);
        projection.Register("UsageRightUsedEvent",     usageRightHandler);
        projection.Register("UsageRightConsumedEvent", usageRightHandler);

        // ── Order / order-change ──────────────────────────────────
        var orderAmendmentHandler = provider.GetRequiredService<OrderAmendmentProj.AmendmentProjectionHandler>();
        projection.Register("AmendmentRequestedEvent", orderAmendmentHandler);
        projection.Register("AmendmentAcceptedEvent",  orderAmendmentHandler);
        projection.Register("AmendmentAppliedEvent",   orderAmendmentHandler);
        projection.Register("AmendmentRejectedEvent",  orderAmendmentHandler);
        projection.Register("AmendmentCancelledEvent", orderAmendmentHandler);

        var cancellationHandler = provider.GetRequiredService<CancellationProjectionHandler>();
        projection.Register("CancellationRequestedEvent", cancellationHandler);
        projection.Register("CancellationConfirmedEvent", cancellationHandler);
        projection.Register("CancellationRejectedEvent",  cancellationHandler);

        var fulfillmentInstructionHandler = provider.GetRequiredService<FulfillmentInstructionProjectionHandler>();
        projection.Register("FulfillmentInstructionCreatedEvent",   fulfillmentInstructionHandler);
        projection.Register("FulfillmentInstructionIssuedEvent",    fulfillmentInstructionHandler);
        projection.Register("FulfillmentInstructionCompletedEvent", fulfillmentInstructionHandler);
        projection.Register("FulfillmentInstructionRevokedEvent",   fulfillmentInstructionHandler);

        // ── Order / order-core ────────────────────────────────────
        var lineItemHandler = provider.GetRequiredService<LineItemProjectionHandler>();
        projection.Register("LineItemCreatedEvent",   lineItemHandler);
        projection.Register("LineItemUpdatedEvent",   lineItemHandler);
        projection.Register("LineItemCancelledEvent", lineItemHandler);

        var orderHandler = provider.GetRequiredService<OrderCoreOrderProj.OrderProjectionHandler>();
        projection.Register("OrderCreatedEvent",   orderHandler);
        projection.Register("OrderConfirmedEvent", orderHandler);
        projection.Register("OrderCompletedEvent", orderHandler);
        projection.Register("OrderCancelledEvent", orderHandler);

        var reservationHandler = provider.GetRequiredService<ReservationProjectionHandler>();
        projection.Register("ReservationHeldEvent",      reservationHandler);
        projection.Register("ReservationConfirmedEvent", reservationHandler);
        projection.Register("ReservationReleasedEvent",  reservationHandler);
        projection.Register("ReservationExpiredEvent",   reservationHandler);

        // ── Service / service-constraint ──────────────────────────
        var policyBindingHandler = provider.GetRequiredService<PolicyBindingProjectionHandler>();
        projection.Register("PolicyBindingCreatedEvent",  policyBindingHandler);
        projection.Register("PolicyBindingBoundEvent",    policyBindingHandler);
        projection.Register("PolicyBindingUnboundEvent",  policyBindingHandler);
        projection.Register("PolicyBindingArchivedEvent", policyBindingHandler);

        var serviceConstraintHandler = provider.GetRequiredService<ServiceConstraintProj.ServiceConstraintProjectionHandler>();
        projection.Register("ServiceConstraintCreatedEvent",   serviceConstraintHandler);
        projection.Register("ServiceConstraintUpdatedEvent",   serviceConstraintHandler);
        projection.Register("ServiceConstraintActivatedEvent", serviceConstraintHandler);
        projection.Register("ServiceConstraintArchivedEvent",  serviceConstraintHandler);

        var serviceWindowHandler = provider.GetRequiredService<ServiceWindowProjectionHandler>();
        projection.Register("ServiceWindowCreatedEvent",   serviceWindowHandler);
        projection.Register("ServiceWindowUpdatedEvent",   serviceWindowHandler);
        projection.Register("ServiceWindowActivatedEvent", serviceWindowHandler);
        projection.Register("ServiceWindowArchivedEvent",  serviceWindowHandler);

        // ── Service / service-core ────────────────────────────────
        var serviceDefinitionHandler = provider.GetRequiredService<ServiceDefinitionProjectionHandler>();
        projection.Register("ServiceDefinitionCreatedEvent",   serviceDefinitionHandler);
        projection.Register("ServiceDefinitionUpdatedEvent",   serviceDefinitionHandler);
        projection.Register("ServiceDefinitionActivatedEvent", serviceDefinitionHandler);
        projection.Register("ServiceDefinitionArchivedEvent",  serviceDefinitionHandler);

        var serviceLevelHandler = provider.GetRequiredService<ServiceLevelProjectionHandler>();
        projection.Register("ServiceLevelCreatedEvent",   serviceLevelHandler);
        projection.Register("ServiceLevelUpdatedEvent",   serviceLevelHandler);
        projection.Register("ServiceLevelActivatedEvent", serviceLevelHandler);
        projection.Register("ServiceLevelArchivedEvent",  serviceLevelHandler);

        var serviceOptionHandler = provider.GetRequiredService<ServiceOptionProjectionHandler>();
        projection.Register("ServiceOptionCreatedEvent",   serviceOptionHandler);
        projection.Register("ServiceOptionUpdatedEvent",   serviceOptionHandler);
        projection.Register("ServiceOptionActivatedEvent", serviceOptionHandler);
        projection.Register("ServiceOptionArchivedEvent",  serviceOptionHandler);

        // ── Provider / provider-core ──────────────────────────────
        var providerCapabilityHandler = provider.GetRequiredService<ProviderCapabilityProjectionHandler>();
        projection.Register("ProviderCapabilityCreatedEvent",   providerCapabilityHandler);
        projection.Register("ProviderCapabilityUpdatedEvent",   providerCapabilityHandler);
        projection.Register("ProviderCapabilityActivatedEvent", providerCapabilityHandler);
        projection.Register("ProviderCapabilityArchivedEvent",  providerCapabilityHandler);

        var providerTierHandler = provider.GetRequiredService<ProviderTierProjectionHandler>();
        projection.Register("ProviderTierCreatedEvent",   providerTierHandler);
        projection.Register("ProviderTierUpdatedEvent",   providerTierHandler);
        projection.Register("ProviderTierActivatedEvent", providerTierHandler);
        projection.Register("ProviderTierArchivedEvent",  providerTierHandler);

        // ── Provider / provider-governance ────────────────────────
        var providerAgreementHandler = provider.GetRequiredService<ProviderAgreementProjectionHandler>();
        projection.Register("ProviderAgreementCreatedEvent",    providerAgreementHandler);
        projection.Register("ProviderAgreementActivatedEvent",  providerAgreementHandler);
        projection.Register("ProviderAgreementSuspendedEvent",  providerAgreementHandler);
        projection.Register("ProviderAgreementTerminatedEvent", providerAgreementHandler);

        // ── Provider / provider-scope ─────────────────────────────
        var providerAvailabilityHandler = provider.GetRequiredService<ProviderAvailabilityProjectionHandler>();
        projection.Register("ProviderAvailabilityCreatedEvent",   providerAvailabilityHandler);
        projection.Register("ProviderAvailabilityUpdatedEvent",   providerAvailabilityHandler);
        projection.Register("ProviderAvailabilityActivatedEvent", providerAvailabilityHandler);
        projection.Register("ProviderAvailabilityArchivedEvent",  providerAvailabilityHandler);

        var providerCoverageHandler = provider.GetRequiredService<ProviderCoverageProjectionHandler>();
        projection.Register("ProviderCoverageCreatedEvent",   providerCoverageHandler);
        projection.Register("CoverageScopeAddedEvent",        providerCoverageHandler);
        projection.Register("CoverageScopeRemovedEvent",      providerCoverageHandler);
        projection.Register("ProviderCoverageActivatedEvent", providerCoverageHandler);
        projection.Register("ProviderCoverageArchivedEvent",  providerCoverageHandler);

        // ── Offering / catalog-core ───────────────────────────────
        var bundleHandler = provider.GetRequiredService<BundleProjectionHandler>();
        projection.Register("BundleCreatedEvent",       bundleHandler);
        projection.Register("BundleMemberAddedEvent",   bundleHandler);
        projection.Register("BundleMemberRemovedEvent", bundleHandler);
        projection.Register("BundleActivatedEvent",     bundleHandler);
        projection.Register("BundleArchivedEvent",      bundleHandler);

        var catalogHandler = provider.GetRequiredService<OfferingCatalogProj.CatalogProjectionHandler>();
        projection.Register("CatalogCreatedEvent",       catalogHandler);
        projection.Register("CatalogMemberAddedEvent",   catalogHandler);
        projection.Register("CatalogMemberRemovedEvent", catalogHandler);
        projection.Register("CatalogPublishedEvent",     catalogHandler);
        projection.Register("CatalogArchivedEvent",      catalogHandler);

        var productHandler = provider.GetRequiredService<ProductProjectionHandler>();
        projection.Register("ProductCreatedEvent",   productHandler);
        projection.Register("ProductUpdatedEvent",   productHandler);
        projection.Register("ProductActivatedEvent", productHandler);
        projection.Register("ProductArchivedEvent",  productHandler);

        var serviceOfferingHandler = provider.GetRequiredService<ServiceOfferingProjectionHandler>();
        projection.Register("ServiceOfferingCreatedEvent",   serviceOfferingHandler);
        projection.Register("ServiceOfferingUpdatedEvent",   serviceOfferingHandler);
        projection.Register("ServiceOfferingActivatedEvent", serviceOfferingHandler);
        projection.Register("ServiceOfferingArchivedEvent",  serviceOfferingHandler);

        // ── Offering / commercial-shape ───────────────────────────
        var configurationHandler = provider.GetRequiredService<ConfigurationProjectionHandler>();
        projection.Register("ConfigurationCreatedEvent",         configurationHandler);
        projection.Register("ConfigurationOptionSetEvent",       configurationHandler);
        projection.Register("ConfigurationOptionRemovedEvent",   configurationHandler);
        projection.Register("ConfigurationActivatedEvent",       configurationHandler);
        projection.Register("ConfigurationArchivedEvent",        configurationHandler);

        var packageHandler = provider.GetRequiredService<PackageProjectionHandler>();
        projection.Register("PackageCreatedEvent",       packageHandler);
        projection.Register("PackageMemberAddedEvent",   packageHandler);
        projection.Register("PackageMemberRemovedEvent", packageHandler);
        projection.Register("PackageActivatedEvent",     packageHandler);
        projection.Register("PackageArchivedEvent",      packageHandler);

        var planHandler = provider.GetRequiredService<PlanProjectionHandler>();
        projection.Register("PlanDraftedEvent",    planHandler);
        projection.Register("PlanActivatedEvent",  planHandler);
        projection.Register("PlanDeprecatedEvent", planHandler);
        projection.Register("PlanArchivedEvent",   planHandler);
    }
}
