using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Amendment.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Approval.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Clause.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Renewal.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Acceptance.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Contract.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Obligation.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Validity.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.PartyGovernance.Counterparty.Application;
using Whycespace.Platform.Host.Composition.Business.Agreement.PartyGovernance.Signature.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Account.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Customer.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Profile.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.ContactPoint.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.Lifecycle.Application;
using Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.Segment.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Assignment.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Eligibility.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Grant.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.Allocation.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.Limit.Application;
using Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.UsageRight.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderChange.Cancellation.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderChange.FulfillmentInstruction.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderCore.LineItem.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderCore.Order.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderCore.Reservation.Application;
using Whycespace.Platform.Host.Composition.Business.Order.OrderChange.Amendment.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.PolicyBinding.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.ServiceConstraint.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.ServiceWindow.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceDefinition.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceLevel.Application;
using Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceOption.Application;
using Whycespace.Platform.Host.Composition.Business.Provider.ProviderCore.ProviderCapability.Application;
using Whycespace.Platform.Host.Composition.Business.Provider.ProviderCore.ProviderTier.Application;
using Whycespace.Platform.Host.Composition.Business.Provider.ProviderGovernance.ProviderAgreement.Application;
using Whycespace.Platform.Host.Composition.Business.Provider.ProviderScope.ProviderAvailability.Application;
using Whycespace.Platform.Host.Composition.Business.Provider.ProviderScope.ProviderCoverage.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Bundle.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Catalog.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Product.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.ServiceOffering.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Configuration.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Package.Application;
using Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Plan.Application;
using OrderAmendmentApp = Whycespace.Platform.Host.Composition.Business.Order.OrderChange.Amendment.Application.AmendmentApplicationModule;
using AgreementAmendmentApp = Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Amendment.Application.AmendmentApplicationModule;
using Whycespace.Platform.Host.Composition.Business.Projection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Business;

/// <summary>
/// Composition root for the business classification. Covers the full agreement
/// context — 10 BCs across 3 domain groups (change-control, commitment,
/// party-governance). Delegates to per-BC application modules and the shared
/// projection + policy modules.
///
/// Intentionally minimal on integration/resilience concerns — no integration
/// workers, no OPA detection, no expiry schedulers, no compensation reactors.
/// Those ship as follow-up when cross-system flows emerge for business.
/// </summary>
public sealed class BusinessSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Application modules — 10 BCs across agreement context.
        services.AddContractApplication();
        services.AddAcceptanceApplication();
        services.AddObligationApplication();
        services.AddValidityApplication();
        services.AddAmendmentApplication();
        services.AddApprovalApplication();
        services.AddClauseApplication();
        services.AddRenewalApplication();
        services.AddCounterpartyApplication();
        services.AddSignatureApplication();

        // Customer context — 6 BCs across identity-and-profile + segmentation-and-lifecycle.
        services.AddAccountApplication();
        services.AddCustomerApplication();
        services.AddProfileApplication();
        services.AddContactPointApplication();
        services.AddLifecycleApplication();
        services.AddSegmentApplication();

        // Entitlement context — 6 BCs across eligibility-and-grant + usage-control.
        services.AddAssignmentApplication();
        services.AddEligibilityApplication();
        services.AddGrantApplication();
        services.AddAllocationApplication();
        services.AddLimitApplication();
        services.AddUsageRightApplication();

        // Order context — 6 BCs across order-change + order-core.
        services.AddOrderChangeAmendmentApplication();
        services.AddCancellationApplication();
        services.AddFulfillmentInstructionApplication();
        services.AddLineItemApplication();
        services.AddOrderApplication();
        services.AddReservationApplication();

        // Service context — 6 BCs across service-constraint + service-core.
        services.AddPolicyBindingApplication();
        services.AddServiceConstraintApplication();
        services.AddServiceWindowApplication();
        services.AddServiceDefinitionApplication();
        services.AddServiceLevelApplication();
        services.AddServiceOptionApplication();

        // Provider context — 5 BCs across provider-core + provider-governance + provider-scope.
        services.AddProviderCapabilityApplication();
        services.AddProviderTierApplication();
        services.AddProviderAgreementApplication();
        services.AddProviderAvailabilityApplication();
        services.AddProviderCoverageApplication();

        // Offering context — 7 BCs across catalog-core + commercial-shape.
        services.AddBundleApplication();
        services.AddCatalogApplication();
        services.AddProductApplication();
        services.AddServiceOfferingApplication();
        services.AddConfigurationApplication();
        services.AddPackageApplication();
        services.AddPlanApplication();

        // Projection wiring (stores + handlers for all 46 BCs).
        services.AddBusinessProjection(configuration);

        // Command → WHYCEPOLICY action bindings for the whole business classification.
        services.AddBusinessPolicyBindings();
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        // Commitment group
        DomainSchemaCatalog.RegisterBusinessAgreementCommitmentContract(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementCommitmentAcceptance(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementCommitmentObligation(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementCommitmentValidity(schema);

        // Change-control group
        DomainSchemaCatalog.RegisterBusinessAgreementChangeControlAmendment(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementChangeControlApproval(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementChangeControlClause(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementChangeControlRenewal(schema);

        // Party-governance group
        DomainSchemaCatalog.RegisterBusinessAgreementPartyGovernanceCounterparty(schema);
        DomainSchemaCatalog.RegisterBusinessAgreementPartyGovernanceSignature(schema);

        // Customer / identity-and-profile
        DomainSchemaCatalog.RegisterBusinessCustomerIdentityAndProfileAccount(schema);
        DomainSchemaCatalog.RegisterBusinessCustomerIdentityAndProfileCustomer(schema);
        DomainSchemaCatalog.RegisterBusinessCustomerIdentityAndProfileProfile(schema);

        // Customer / segmentation-and-lifecycle
        DomainSchemaCatalog.RegisterBusinessCustomerSegmentationAndLifecycleContactPoint(schema);
        DomainSchemaCatalog.RegisterBusinessCustomerSegmentationAndLifecycleLifecycle(schema);
        DomainSchemaCatalog.RegisterBusinessCustomerSegmentationAndLifecycleSegment(schema);

        // Entitlement / eligibility-and-grant
        DomainSchemaCatalog.RegisterBusinessEntitlementEligibilityAndGrantAssignment(schema);
        DomainSchemaCatalog.RegisterBusinessEntitlementEligibilityAndGrantEligibility(schema);
        DomainSchemaCatalog.RegisterBusinessEntitlementEligibilityAndGrantGrant(schema);

        // Entitlement / usage-control
        DomainSchemaCatalog.RegisterBusinessEntitlementUsageControlAllocation(schema);
        DomainSchemaCatalog.RegisterBusinessEntitlementUsageControlLimit(schema);
        DomainSchemaCatalog.RegisterBusinessEntitlementUsageControlUsageRight(schema);

        // Order / order-change
        DomainSchemaCatalog.RegisterBusinessOrderOrderChangeAmendment(schema);
        DomainSchemaCatalog.RegisterBusinessOrderOrderChangeCancellation(schema);
        DomainSchemaCatalog.RegisterBusinessOrderOrderChangeFulfillmentInstruction(schema);

        // Order / order-core
        DomainSchemaCatalog.RegisterBusinessOrderOrderCoreLineItem(schema);
        DomainSchemaCatalog.RegisterBusinessOrderOrderCoreOrder(schema);
        DomainSchemaCatalog.RegisterBusinessOrderOrderCoreReservation(schema);

        // Service / service-constraint
        DomainSchemaCatalog.RegisterBusinessServiceServiceConstraintPolicyBinding(schema);
        DomainSchemaCatalog.RegisterBusinessServiceServiceConstraintServiceConstraint(schema);
        DomainSchemaCatalog.RegisterBusinessServiceServiceConstraintServiceWindow(schema);

        // Service / service-core
        DomainSchemaCatalog.RegisterBusinessServiceServiceCoreServiceDefinition(schema);
        DomainSchemaCatalog.RegisterBusinessServiceServiceCoreServiceLevel(schema);
        DomainSchemaCatalog.RegisterBusinessServiceServiceCoreServiceOption(schema);

        // Provider / provider-core
        DomainSchemaCatalog.RegisterBusinessProviderProviderCoreProviderCapability(schema);
        DomainSchemaCatalog.RegisterBusinessProviderProviderCoreProviderTier(schema);

        // Provider / provider-governance
        DomainSchemaCatalog.RegisterBusinessProviderProviderGovernanceProviderAgreement(schema);

        // Provider / provider-scope
        DomainSchemaCatalog.RegisterBusinessProviderProviderScopeProviderAvailability(schema);
        DomainSchemaCatalog.RegisterBusinessProviderProviderScopeProviderCoverage(schema);

        // Offering / catalog-core
        DomainSchemaCatalog.RegisterBusinessOfferingCatalogCoreBundle(schema);
        DomainSchemaCatalog.RegisterBusinessOfferingCatalogCoreCatalog(schema);
        DomainSchemaCatalog.RegisterBusinessOfferingCatalogCoreProduct(schema);
        DomainSchemaCatalog.RegisterBusinessOfferingCatalogCoreServiceOffering(schema);

        // Offering / commercial-shape
        DomainSchemaCatalog.RegisterBusinessOfferingCommercialShapeConfiguration(schema);
        DomainSchemaCatalog.RegisterBusinessOfferingCommercialShapePackage(schema);
        DomainSchemaCatalog.RegisterBusinessOfferingCommercialShapePlan(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        BusinessProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        ContractApplicationModule.RegisterEngines(engine);
        AcceptanceApplicationModule.RegisterEngines(engine);
        ObligationApplicationModule.RegisterEngines(engine);
        ValidityApplicationModule.RegisterEngines(engine);
        AgreementAmendmentApp.RegisterEngines(engine);
        ApprovalApplicationModule.RegisterEngines(engine);
        ClauseApplicationModule.RegisterEngines(engine);
        RenewalApplicationModule.RegisterEngines(engine);
        CounterpartyApplicationModule.RegisterEngines(engine);
        SignatureApplicationModule.RegisterEngines(engine);

        // Customer context
        AccountApplicationModule.RegisterEngines(engine);
        CustomerApplicationModule.RegisterEngines(engine);
        ProfileApplicationModule.RegisterEngines(engine);
        ContactPointApplicationModule.RegisterEngines(engine);
        LifecycleApplicationModule.RegisterEngines(engine);
        SegmentApplicationModule.RegisterEngines(engine);

        // Entitlement context
        AssignmentApplicationModule.RegisterEngines(engine);
        EligibilityApplicationModule.RegisterEngines(engine);
        GrantApplicationModule.RegisterEngines(engine);
        AllocationApplicationModule.RegisterEngines(engine);
        LimitApplicationModule.RegisterEngines(engine);
        UsageRightApplicationModule.RegisterEngines(engine);

        // Order context
        OrderAmendmentApp.RegisterEngines(engine);
        CancellationApplicationModule.RegisterEngines(engine);
        FulfillmentInstructionApplicationModule.RegisterEngines(engine);
        LineItemApplicationModule.RegisterEngines(engine);
        OrderApplicationModule.RegisterEngines(engine);
        ReservationApplicationModule.RegisterEngines(engine);

        // Service context
        PolicyBindingApplicationModule.RegisterEngines(engine);
        ServiceConstraintApplicationModule.RegisterEngines(engine);
        ServiceWindowApplicationModule.RegisterEngines(engine);
        ServiceDefinitionApplicationModule.RegisterEngines(engine);
        ServiceLevelApplicationModule.RegisterEngines(engine);
        ServiceOptionApplicationModule.RegisterEngines(engine);

        // Provider context
        ProviderCapabilityApplicationModule.RegisterEngines(engine);
        ProviderTierApplicationModule.RegisterEngines(engine);
        ProviderAgreementApplicationModule.RegisterEngines(engine);
        ProviderAvailabilityApplicationModule.RegisterEngines(engine);
        ProviderCoverageApplicationModule.RegisterEngines(engine);

        // Offering context
        BundleApplicationModule.RegisterEngines(engine);
        CatalogApplicationModule.RegisterEngines(engine);
        ProductApplicationModule.RegisterEngines(engine);
        ServiceOfferingApplicationModule.RegisterEngines(engine);
        ConfigurationApplicationModule.RegisterEngines(engine);
        PackageApplicationModule.RegisterEngines(engine);
        PlanApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows in the agreement context — every command is single-shot T2E.
    }
}
