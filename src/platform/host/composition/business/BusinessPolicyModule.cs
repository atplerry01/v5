using Microsoft.Extensions.DependencyInjection;
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
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using OrderAmendment = Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using ServiceConstraintBc = Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
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
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Business;

/// <summary>
/// Business classification policy bindings. One <see cref="CommandPolicyBinding"/>
/// per command, mapping CLR type to canonical policy id.
///
/// Coverage: agreement context, 10 BCs, 32 commands total.
/// </summary>
public static class BusinessPolicyModule
{
    public static IServiceCollection AddBusinessPolicyBindings(this IServiceCollection services)
    {
        // ── agreement/commitment/contract (5) ─────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateContractCommand),     ContractPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddPartyToContractCommand), ContractPolicyIds.AddParty));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateContractCommand),   ContractPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(SuspendContractCommand),    ContractPolicyIds.Suspend));
        services.AddSingleton(new CommandPolicyBinding(typeof(TerminateContractCommand),  ContractPolicyIds.Terminate));

        // ── agreement/commitment/acceptance (3) ───────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAcceptanceCommand),   AcceptancePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AcceptAcceptanceCommand),   AcceptancePolicyIds.Accept));
        services.AddSingleton(new CommandPolicyBinding(typeof(RejectAcceptanceCommand),   AcceptancePolicyIds.Reject));

        // ── agreement/commitment/obligation (3) ───────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateObligationCommand),   ObligationPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(FulfillObligationCommand),  ObligationPolicyIds.Fulfill));
        services.AddSingleton(new CommandPolicyBinding(typeof(BreachObligationCommand),   ObligationPolicyIds.Breach));

        // ── agreement/commitment/validity (3) ─────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateValidityCommand),     ValidityPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireValidityCommand),     ValidityPolicyIds.Expire));
        services.AddSingleton(new CommandPolicyBinding(typeof(InvalidateValidityCommand), ValidityPolicyIds.Invalidate));

        // ── agreement/change-control/amendment (3) ────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAmendmentCommand),    AmendmentPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ApplyAmendmentCommand),     AmendmentPolicyIds.Apply));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevertAmendmentCommand),    AmendmentPolicyIds.Revert));

        // ── agreement/change-control/approval (3) ─────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateApprovalCommand),     ApprovalPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ApproveApprovalCommand),    ApprovalPolicyIds.Approve));
        services.AddSingleton(new CommandPolicyBinding(typeof(RejectApprovalCommand),     ApprovalPolicyIds.Reject));

        // ── agreement/change-control/clause (3) ───────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateClauseCommand),       ClausePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateClauseCommand),     ClausePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(SupersedeClauseCommand),    ClausePolicyIds.Supersede));

        // ── agreement/change-control/renewal (3) ──────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateRenewalCommand),      RenewalPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RenewRenewalCommand),       RenewalPolicyIds.Renew));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireRenewalCommand),      RenewalPolicyIds.Expire));

        // ── agreement/party-governance/counterparty (3) ───────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCounterpartyCommand),    CounterpartyPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(SuspendCounterpartyCommand),   CounterpartyPolicyIds.Suspend));
        services.AddSingleton(new CommandPolicyBinding(typeof(TerminateCounterpartyCommand), CounterpartyPolicyIds.Terminate));

        // ── agreement/party-governance/signature (3) ──────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateSignatureCommand),    SignaturePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(SignSignatureCommand),      SignaturePolicyIds.Sign));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeSignatureCommand),    SignaturePolicyIds.Revoke));

        // ── customer/identity-and-profile/account (5) ─────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAccountCommand),      AccountPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RenameAccountCommand),      AccountPolicyIds.Rename));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateAccountCommand),    AccountPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(SuspendAccountCommand),     AccountPolicyIds.Suspend));
        services.AddSingleton(new CommandPolicyBinding(typeof(CloseAccountCommand),       AccountPolicyIds.Close));

        // ── customer/identity-and-profile/customer (5) ────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCustomerCommand),     CustomerPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RenameCustomerCommand),     CustomerPolicyIds.Rename));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReclassifyCustomerCommand), CustomerPolicyIds.Reclassify));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateCustomerCommand),   CustomerPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveCustomerCommand),    CustomerPolicyIds.Archive));

        // ── customer/identity-and-profile/profile (6) ─────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProfileCommand),           ProfilePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RenameProfileCommand),           ProfilePolicyIds.Rename));
        services.AddSingleton(new CommandPolicyBinding(typeof(SetProfileDescriptorCommand),    ProfilePolicyIds.SetDescriptor));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveProfileDescriptorCommand), ProfilePolicyIds.RemoveDescriptor));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProfileCommand),         ProfilePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProfileCommand),          ProfilePolicyIds.Archive));

        // ── customer/segmentation-and-lifecycle/contact-point (5) ─
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateContactPointCommand),       ContactPointPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateContactPointCommand),       ContactPointPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateContactPointCommand),     ContactPointPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(SetContactPointPreferredCommand), ContactPointPolicyIds.SetPreferred));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveContactPointCommand),      ContactPointPolicyIds.Archive));

        // ── customer/segmentation-and-lifecycle/lifecycle (3) ─────
        services.AddSingleton(new CommandPolicyBinding(typeof(StartLifecycleCommand),       LifecyclePolicyIds.Start));
        services.AddSingleton(new CommandPolicyBinding(typeof(ChangeLifecycleStageCommand), LifecyclePolicyIds.ChangeStage));
        services.AddSingleton(new CommandPolicyBinding(typeof(CloseLifecycleCommand),       LifecyclePolicyIds.Close));

        // ── customer/segmentation-and-lifecycle/segment (4) ───────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateSegmentCommand),   SegmentPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateSegmentCommand),   SegmentPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateSegmentCommand), SegmentPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveSegmentCommand),  SegmentPolicyIds.Archive));

        // ── entitlement/eligibility-and-grant/assignment (3) ──────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAssignmentCommand),   AssignmentPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateAssignmentCommand), AssignmentPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeAssignmentCommand),   AssignmentPolicyIds.Revoke));

        // ── entitlement/eligibility-and-grant/eligibility (3) ─────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateEligibilityCommand), EligibilityPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkEligibleCommand),      EligibilityPolicyIds.MarkEligible));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkIneligibleCommand),    EligibilityPolicyIds.MarkIneligible));

        // ── entitlement/eligibility-and-grant/grant (4) ───────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateGrantCommand),   GrantPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateGrantCommand), GrantPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeGrantCommand),   GrantPolicyIds.Revoke));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireGrantCommand),   GrantPolicyIds.Expire));

        // ── entitlement/usage-control/allocation (3) ──────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAllocationCommand),   AllocationPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AllocateAllocationCommand), AllocationPolicyIds.Allocate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseAllocationCommand),  AllocationPolicyIds.Release));

        // ── entitlement/usage-control/limit (3) ───────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateLimitCommand),   LimitPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(EnforceLimitCommand),  LimitPolicyIds.Enforce));
        services.AddSingleton(new CommandPolicyBinding(typeof(BreachLimitCommand),   LimitPolicyIds.Breach));

        // ── entitlement/usage-control/usage-right (3) ─────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateUsageRightCommand),  UsageRightPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UseUsageRightCommand),     UsageRightPolicyIds.Use));
        services.AddSingleton(new CommandPolicyBinding(typeof(ConsumeUsageRightCommand), UsageRightPolicyIds.Consume));

        // ── order/order-change/amendment (5, aliased) ─────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(OrderAmendment.RequestAmendmentCommand), OrderAmendment.AmendmentPolicyIds.Request));
        services.AddSingleton(new CommandPolicyBinding(typeof(OrderAmendment.AcceptAmendmentCommand),  OrderAmendment.AmendmentPolicyIds.Accept));
        services.AddSingleton(new CommandPolicyBinding(typeof(OrderAmendment.ApplyAmendmentCommand),   OrderAmendment.AmendmentPolicyIds.Apply));
        services.AddSingleton(new CommandPolicyBinding(typeof(OrderAmendment.RejectAmendmentCommand),  OrderAmendment.AmendmentPolicyIds.Reject));
        services.AddSingleton(new CommandPolicyBinding(typeof(OrderAmendment.CancelAmendmentCommand),  OrderAmendment.AmendmentPolicyIds.Cancel));

        // ── order/order-change/cancellation (3) ───────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(RequestCancellationCommand), CancellationPolicyIds.Request));
        services.AddSingleton(new CommandPolicyBinding(typeof(ConfirmCancellationCommand), CancellationPolicyIds.Confirm));
        services.AddSingleton(new CommandPolicyBinding(typeof(RejectCancellationCommand),  CancellationPolicyIds.Reject));

        // ── order/order-change/fulfillment-instruction (4) ────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateFulfillmentInstructionCommand),   FulfillmentInstructionPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(IssueFulfillmentInstructionCommand),    FulfillmentInstructionPolicyIds.Issue));
        services.AddSingleton(new CommandPolicyBinding(typeof(CompleteFulfillmentInstructionCommand), FulfillmentInstructionPolicyIds.Complete));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeFulfillmentInstructionCommand),   FulfillmentInstructionPolicyIds.Revoke));

        // ── order/order-core/line-item (3) ────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateLineItemCommand),         LineItemPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateLineItemQuantityCommand), LineItemPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(CancelLineItemCommand),         LineItemPolicyIds.Cancel));

        // ── order/order-core/order (4) ────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateOrderCommand),   OrderPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ConfirmOrderCommand),  OrderPolicyIds.Confirm));
        services.AddSingleton(new CommandPolicyBinding(typeof(CompleteOrderCommand), OrderPolicyIds.Complete));
        services.AddSingleton(new CommandPolicyBinding(typeof(CancelOrderCommand),   OrderPolicyIds.Cancel));

        // ── order/order-core/reservation (4) ──────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(HoldReservationCommand),    ReservationPolicyIds.Hold));
        services.AddSingleton(new CommandPolicyBinding(typeof(ConfirmReservationCommand), ReservationPolicyIds.Confirm));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseReservationCommand), ReservationPolicyIds.Release));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireReservationCommand),  ReservationPolicyIds.Expire));

        // ── service/service-constraint/policy-binding (4) ─────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreatePolicyBindingCommand),  PolicyBindingPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(BindPolicyBindingCommand),    PolicyBindingPolicyIds.Bind));
        services.AddSingleton(new CommandPolicyBinding(typeof(UnbindPolicyBindingCommand),  PolicyBindingPolicyIds.Unbind));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchivePolicyBindingCommand), PolicyBindingPolicyIds.Archive));

        // ── service/service-constraint/service-constraint (4) ─────
        services.AddSingleton(new CommandPolicyBinding(typeof(ServiceConstraintBc.CreateServiceConstraintCommand),   ServiceConstraintBc.ServiceConstraintPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ServiceConstraintBc.UpdateServiceConstraintCommand),   ServiceConstraintBc.ServiceConstraintPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ServiceConstraintBc.ActivateServiceConstraintCommand), ServiceConstraintBc.ServiceConstraintPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ServiceConstraintBc.ArchiveServiceConstraintCommand),  ServiceConstraintBc.ServiceConstraintPolicyIds.Archive));

        // ── service/service-constraint/service-window (4) ─────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateServiceWindowCommand),   ServiceWindowPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateServiceWindowCommand),   ServiceWindowPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateServiceWindowCommand), ServiceWindowPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveServiceWindowCommand),  ServiceWindowPolicyIds.Archive));

        // ── service/service-core/service-definition (4) ───────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateServiceDefinitionCommand),   ServiceDefinitionPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateServiceDefinitionCommand),   ServiceDefinitionPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateServiceDefinitionCommand), ServiceDefinitionPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveServiceDefinitionCommand),  ServiceDefinitionPolicyIds.Archive));

        // ── service/service-core/service-level (4) ────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateServiceLevelCommand),   ServiceLevelPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateServiceLevelCommand),   ServiceLevelPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateServiceLevelCommand), ServiceLevelPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveServiceLevelCommand),  ServiceLevelPolicyIds.Archive));

        // ── service/service-core/service-option (4) ───────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateServiceOptionCommand),   ServiceOptionPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateServiceOptionCommand),   ServiceOptionPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateServiceOptionCommand), ServiceOptionPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveServiceOptionCommand),  ServiceOptionPolicyIds.Archive));

        // ── provider/provider-core/provider-capability (4) ────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProviderCapabilityCommand),   ProviderCapabilityPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateProviderCapabilityCommand),   ProviderCapabilityPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProviderCapabilityCommand), ProviderCapabilityPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProviderCapabilityCommand),  ProviderCapabilityPolicyIds.Archive));

        // ── provider/provider-core/provider-tier (4) ──────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProviderTierCommand),   ProviderTierPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateProviderTierCommand),   ProviderTierPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProviderTierCommand), ProviderTierPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProviderTierCommand),  ProviderTierPolicyIds.Archive));

        // ── provider/provider-governance/provider-agreement (4) ───
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProviderAgreementCommand),    ProviderAgreementPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProviderAgreementCommand),  ProviderAgreementPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(SuspendProviderAgreementCommand),   ProviderAgreementPolicyIds.Suspend));
        services.AddSingleton(new CommandPolicyBinding(typeof(TerminateProviderAgreementCommand), ProviderAgreementPolicyIds.Terminate));

        // ── provider/provider-scope/provider-availability (4) ─────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProviderAvailabilityCommand),       ProviderAvailabilityPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateProviderAvailabilityWindowCommand), ProviderAvailabilityPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProviderAvailabilityCommand),     ProviderAvailabilityPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProviderAvailabilityCommand),      ProviderAvailabilityPolicyIds.Archive));

        // ── provider/provider-scope/provider-coverage (5) ─────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProviderCoverageCommand),   ProviderCoveragePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddCoverageScopeCommand),         ProviderCoveragePolicyIds.AddScope));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveCoverageScopeCommand),      ProviderCoveragePolicyIds.RemoveScope));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProviderCoverageCommand), ProviderCoveragePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProviderCoverageCommand),  ProviderCoveragePolicyIds.Archive));

        // ── offering/catalog-core/bundle (5) ──────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateBundleCommand),       BundlePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddBundleMemberCommand),    BundlePolicyIds.AddMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveBundleMemberCommand), BundlePolicyIds.RemoveMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateBundleCommand),     BundlePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveBundleCommand),      BundlePolicyIds.Archive));

        // ── offering/catalog-core/catalog (5) ─────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCatalogCommand),       CatalogPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddCatalogMemberCommand),    CatalogPolicyIds.AddMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveCatalogMemberCommand), CatalogPolicyIds.RemoveMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(PublishCatalogCommand),      CatalogPolicyIds.Publish));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveCatalogCommand),      CatalogPolicyIds.Archive));

        // ── offering/catalog-core/product (4) ─────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProductCommand),   ProductPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateProductCommand),   ProductPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProductCommand), ProductPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveProductCommand),  ProductPolicyIds.Archive));

        // ── offering/catalog-core/service-offering (4) ────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateServiceOfferingCommand),   ServiceOfferingPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateServiceOfferingCommand),   ServiceOfferingPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateServiceOfferingCommand), ServiceOfferingPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveServiceOfferingCommand),  ServiceOfferingPolicyIds.Archive));

        // ── offering/commercial-shape/configuration (5) ───────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateConfigurationCommand),          ConfigurationPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(SetConfigurationOptionCommand),       ConfigurationPolicyIds.SetOption));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveConfigurationOptionCommand),    ConfigurationPolicyIds.RemoveOption));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateConfigurationCommand),        ConfigurationPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchiveConfigurationCommand),         ConfigurationPolicyIds.Archive));

        // ── offering/commercial-shape/package (5) ─────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreatePackageCommand),       PackagePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddPackageMemberCommand),    PackagePolicyIds.AddMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemovePackageMemberCommand), PackagePolicyIds.RemoveMember));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivatePackageCommand),     PackagePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchivePackageCommand),      PackagePolicyIds.Archive));

        // ── offering/commercial-shape/plan (4) ────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DraftPlanCommand),     PlanPolicyIds.Draft));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivatePlanCommand),  PlanPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(DeprecatePlanCommand), PlanPolicyIds.Deprecate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ArchivePlanCommand),   PlanPolicyIds.Archive));

        return services;
    }
}
