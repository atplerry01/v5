namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Host-facing dispatcher for per-domain schema modules.
///
/// Phase 1.5 §5.1.2 BPV-D01 seam: host composition modules call the static
/// methods on this catalog instead of typing domain event classes directly.
/// Each method is a strongly-typed, one-line dispatch into a per-domain
/// <see cref="ISchemaModule"/> via an <see cref="EventSchemaRegistrySink"/>.
///
/// Adding a new domain means adding a new <c>*SchemaModule</c> next to this
/// file plus a one-line <c>RegisterX</c> method here. Host bootstraps never
/// learn the domain CLR types.
/// </summary>
public static class DomainSchemaCatalog
{
    public static void RegisterConstitutionalPolicyDecision(EventSchemaRegistry registry)
        => new PolicyDecisionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterConstitutionalChainAnchorRecord(EventSchemaRegistry registry)
        => new ConstitutionalChainAnchorRecordSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterConstitutionalChainEvidenceRecord(EventSchemaRegistry registry)
        => new ConstitutionalChainEvidenceRecordSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterConstitutionalChainLedger(EventSchemaRegistry registry)
        => new ConstitutionalChainLedgerSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterOrchestrationWorkflowExecution(EventSchemaRegistry registry)
        => new WorkflowExecutionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterEconomic(EventSchemaRegistry registry)
        => new EconomicSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterEconomicTransaction(EventSchemaRegistry registry)
        => new TransactionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>R3.B.1 — integration/outbound-effect lifecycle schema bindings.</summary>
    public static void RegisterIntegrationOutboundEffect(EventSchemaRegistry registry)
        => new OutboundEffectSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Content-system vertical — document context BC schema bindings.
    /// Full E1→EX delivery across 10 populated BCs (2026-04-21).
    /// </summary>
    public static void RegisterContentDocumentCoreObjectDocument(EventSchemaRegistry registry)
        => new ContentDocumentCoreObjectDocumentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentCoreObjectBundle(EventSchemaRegistry registry)
        => new ContentDocumentCoreObjectBundleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentCoreObjectFile(EventSchemaRegistry registry)
        => new ContentDocumentCoreObjectFileSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentCoreObjectRecord(EventSchemaRegistry registry)
        => new ContentDocumentCoreObjectRecordSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentCoreObjectTemplate(EventSchemaRegistry registry)
        => new ContentDocumentCoreObjectTemplateSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentDescriptorMetadata(EventSchemaRegistry registry)
        => new ContentDocumentDescriptorMetadataSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentGovernanceRetention(EventSchemaRegistry registry)
        => new ContentDocumentGovernanceRetentionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentIntakeUpload(EventSchemaRegistry registry)
        => new ContentDocumentIntakeUploadSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentLifecycleChangeProcessing(EventSchemaRegistry registry)
        => new ContentDocumentLifecycleChangeProcessingSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentDocumentLifecycleChangeVersion(EventSchemaRegistry registry)
        => new ContentDocumentLifecycleChangeVersionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Content-system vertical — media context BC schema bindings.
    /// Full E1→EX delivery across 7 populated media BCs (2026-04-21).
    /// </summary>
    public static void RegisterContentMediaCoreObjectAsset(EventSchemaRegistry registry)
        => new ContentMediaCoreObjectAssetSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaCoreObjectSubtitle(EventSchemaRegistry registry)
        => new ContentMediaCoreObjectSubtitleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaCoreObjectTranscript(EventSchemaRegistry registry)
        => new ContentMediaCoreObjectTranscriptSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaDescriptorMetadata(EventSchemaRegistry registry)
        => new ContentMediaDescriptorMetadataSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaIntakeIngest(EventSchemaRegistry registry)
        => new ContentMediaIntakeIngestSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaLifecycleChangeVersion(EventSchemaRegistry registry)
        => new ContentMediaLifecycleChangeVersionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentMediaTechnicalProcessingProcessing(EventSchemaRegistry registry)
        => new ContentMediaTechnicalProcessingProcessingSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Content-system vertical — streaming context BC schema bindings.
    /// Full E1→EX delivery across 9 populated streaming BCs
    /// (stream-core: stream/channel/manifest/availability; live-streaming:
    /// broadcast/archive; playback-consumption: session; delivery-governance:
    /// access/observability).
    /// </summary>
    public static void RegisterContentStreamingStreamCoreStream(EventSchemaRegistry registry)
        => new ContentStreamingStreamCoreStreamSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingStreamCoreChannel(EventSchemaRegistry registry)
        => new ContentStreamingStreamCoreChannelSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingStreamCoreManifest(EventSchemaRegistry registry)
        => new ContentStreamingStreamCoreManifestSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingStreamCoreAvailability(EventSchemaRegistry registry)
        => new ContentStreamingStreamCoreAvailabilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingLiveStreamingBroadcast(EventSchemaRegistry registry)
        => new ContentStreamingLiveStreamingBroadcastSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingLiveStreamingArchive(EventSchemaRegistry registry)
        => new ContentStreamingLiveStreamingArchiveSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingPlaybackConsumptionSession(EventSchemaRegistry registry)
        => new ContentStreamingPlaybackConsumptionSessionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingDeliveryGovernanceAccess(EventSchemaRegistry registry)
        => new ContentStreamingDeliveryGovernanceAccessSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingDeliveryGovernanceObservability(EventSchemaRegistry registry)
        => new ContentStreamingDeliveryGovernanceObservabilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingDeliveryGovernanceEntitlementHook(EventSchemaRegistry registry)
        => new ContentStreamingDeliveryGovernanceEntitlementHookSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingDeliveryGovernanceModeration(EventSchemaRegistry registry)
        => new ContentStreamingDeliveryGovernanceModerationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingLiveStreamingIngestSession(EventSchemaRegistry registry)
        => new ContentStreamingLiveStreamingIngestSessionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingPlaybackConsumptionProgress(EventSchemaRegistry registry)
        => new ContentStreamingPlaybackConsumptionProgressSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterContentStreamingPlaybackConsumptionReplay(EventSchemaRegistry registry)
        => new ContentStreamingPlaybackConsumptionReplaySchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Structural-system vertical — structure context E1→Ex delivery (phase 1).
    /// Future phases: cluster (phase 2), humancapital (phase 3).
    /// </summary>
    public static void RegisterStructuralStructureTypeDefinition(EventSchemaRegistry registry)
        => new StructuralStructureTypeDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralStructureClassification(EventSchemaRegistry registry)
        => new StructuralStructureClassificationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralStructureHierarchyDefinition(EventSchemaRegistry registry)
        => new StructuralStructureHierarchyDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralStructureTopologyDefinition(EventSchemaRegistry registry)
        => new StructuralStructureTopologyDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Structural-system vertical — cluster context E1→Ex delivery (phase 2).
    /// 8 populated BCs: administration, authority, cluster, lifecycle,
    /// provider, spv, subcluster, topology.
    /// </summary>
    public static void RegisterStructuralClusterAdministration(EventSchemaRegistry registry)
        => new StructuralClusterAdministrationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterAuthority(EventSchemaRegistry registry)
        => new StructuralClusterAuthoritySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterCluster(EventSchemaRegistry registry)
        => new StructuralClusterClusterSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterLifecycle(EventSchemaRegistry registry)
        => new StructuralClusterLifecycleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterProvider(EventSchemaRegistry registry)
        => new StructuralClusterProviderSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterSpv(EventSchemaRegistry registry)
        => new StructuralClusterSpvSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterSubcluster(EventSchemaRegistry registry)
        => new StructuralClusterSubclusterSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralClusterTopology(EventSchemaRegistry registry)
        => new StructuralClusterTopologySchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Structural-system vertical — humancapital context E1→Ex delivery
    /// (phase 3a, partial). Only BCs with callable event-raising paths:
    /// participant (Register + Place) and assignment (Assign). The 10
    /// remaining humancapital BCs remain at D0 pending domain promotion.
    /// </summary>
    public static void RegisterStructuralHumancapitalParticipant(EventSchemaRegistry registry)
        => new StructuralHumancapitalParticipantSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalAssignment(EventSchemaRegistry registry)
        => new StructuralHumancapitalAssignmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalEligibility(EventSchemaRegistry registry)
        => new StructuralHumancapitalEligibilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalGovernance(EventSchemaRegistry registry)
        => new StructuralHumancapitalGovernanceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalIncentive(EventSchemaRegistry registry)
        => new StructuralHumancapitalIncentiveSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalOperator(EventSchemaRegistry registry)
        => new StructuralHumancapitalOperatorSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalPerformance(EventSchemaRegistry registry)
        => new StructuralHumancapitalPerformanceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalReputation(EventSchemaRegistry registry)
        => new StructuralHumancapitalReputationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalSanction(EventSchemaRegistry registry)
        => new StructuralHumancapitalSanctionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalSponsorship(EventSchemaRegistry registry)
        => new StructuralHumancapitalSponsorshipSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalStewardship(EventSchemaRegistry registry)
        => new StructuralHumancapitalStewardshipSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterStructuralHumancapitalWorkforce(EventSchemaRegistry registry)
        => new StructuralHumancapitalWorkforceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — agreement context E1→Ex delivery (10 BCs across 3 groups).
    /// </summary>
    public static void RegisterBusinessAgreementCommitmentContract(EventSchemaRegistry registry)
        => new BusinessAgreementCommitmentContractSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementCommitmentAcceptance(EventSchemaRegistry registry)
        => new BusinessAgreementCommitmentAcceptanceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementCommitmentObligation(EventSchemaRegistry registry)
        => new BusinessAgreementCommitmentObligationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementCommitmentValidity(EventSchemaRegistry registry)
        => new BusinessAgreementCommitmentValiditySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementChangeControlAmendment(EventSchemaRegistry registry)
        => new BusinessAgreementChangeControlAmendmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementChangeControlApproval(EventSchemaRegistry registry)
        => new BusinessAgreementChangeControlApprovalSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementChangeControlClause(EventSchemaRegistry registry)
        => new BusinessAgreementChangeControlClauseSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementChangeControlRenewal(EventSchemaRegistry registry)
        => new BusinessAgreementChangeControlRenewalSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementPartyGovernanceCounterparty(EventSchemaRegistry registry)
        => new BusinessAgreementPartyGovernanceCounterpartySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessAgreementPartyGovernanceSignature(EventSchemaRegistry registry)
        => new BusinessAgreementPartyGovernanceSignatureSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — customer context E1→Ex delivery (6 BCs across 2 groups).
    /// </summary>
    public static void RegisterBusinessCustomerIdentityAndProfileAccount(EventSchemaRegistry registry)
        => new BusinessCustomerIdentityAndProfileAccountSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessCustomerIdentityAndProfileCustomer(EventSchemaRegistry registry)
        => new BusinessCustomerIdentityAndProfileCustomerSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessCustomerIdentityAndProfileProfile(EventSchemaRegistry registry)
        => new BusinessCustomerIdentityAndProfileProfileSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessCustomerSegmentationAndLifecycleContactPoint(EventSchemaRegistry registry)
        => new BusinessCustomerSegmentationAndLifecycleContactPointSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessCustomerSegmentationAndLifecycleLifecycle(EventSchemaRegistry registry)
        => new BusinessCustomerSegmentationAndLifecycleLifecycleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessCustomerSegmentationAndLifecycleSegment(EventSchemaRegistry registry)
        => new BusinessCustomerSegmentationAndLifecycleSegmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — entitlement context E1→Ex delivery (6 BCs across 2 groups).
    /// </summary>
    public static void RegisterBusinessEntitlementEligibilityAndGrantAssignment(EventSchemaRegistry registry)
        => new BusinessEntitlementEligibilityAndGrantAssignmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessEntitlementEligibilityAndGrantEligibility(EventSchemaRegistry registry)
        => new BusinessEntitlementEligibilityAndGrantEligibilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessEntitlementEligibilityAndGrantGrant(EventSchemaRegistry registry)
        => new BusinessEntitlementEligibilityAndGrantGrantSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessEntitlementUsageControlAllocation(EventSchemaRegistry registry)
        => new BusinessEntitlementUsageControlAllocationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessEntitlementUsageControlLimit(EventSchemaRegistry registry)
        => new BusinessEntitlementUsageControlLimitSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessEntitlementUsageControlUsageRight(EventSchemaRegistry registry)
        => new BusinessEntitlementUsageControlUsageRightSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — order context E1→Ex delivery (6 BCs across 2 groups).
    /// </summary>
    public static void RegisterBusinessOrderOrderChangeAmendment(EventSchemaRegistry registry)
        => new BusinessOrderOrderChangeAmendmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOrderOrderChangeCancellation(EventSchemaRegistry registry)
        => new BusinessOrderOrderChangeCancellationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOrderOrderChangeFulfillmentInstruction(EventSchemaRegistry registry)
        => new BusinessOrderOrderChangeFulfillmentInstructionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOrderOrderCoreLineItem(EventSchemaRegistry registry)
        => new BusinessOrderOrderCoreLineItemSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOrderOrderCoreOrder(EventSchemaRegistry registry)
        => new BusinessOrderOrderCoreOrderSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOrderOrderCoreReservation(EventSchemaRegistry registry)
        => new BusinessOrderOrderCoreReservationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — service context E1→Ex delivery (6 BCs across 2 groups).
    /// </summary>
    public static void RegisterBusinessServiceServiceConstraintPolicyBinding(EventSchemaRegistry registry)
        => new BusinessServiceServiceConstraintPolicyBindingSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessServiceServiceConstraintServiceConstraint(EventSchemaRegistry registry)
        => new BusinessServiceServiceConstraintServiceConstraintSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessServiceServiceConstraintServiceWindow(EventSchemaRegistry registry)
        => new BusinessServiceServiceConstraintServiceWindowSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessServiceServiceCoreServiceDefinition(EventSchemaRegistry registry)
        => new BusinessServiceServiceCoreServiceDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessServiceServiceCoreServiceLevel(EventSchemaRegistry registry)
        => new BusinessServiceServiceCoreServiceLevelSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessServiceServiceCoreServiceOption(EventSchemaRegistry registry)
        => new BusinessServiceServiceCoreServiceOptionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — provider context E1→Ex delivery (5 BCs across 3 groups).
    /// </summary>
    public static void RegisterBusinessProviderProviderCoreProviderCapability(EventSchemaRegistry registry)
        => new BusinessProviderProviderCoreProviderCapabilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessProviderProviderCoreProviderTier(EventSchemaRegistry registry)
        => new BusinessProviderProviderCoreProviderTierSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessProviderProviderGovernanceProviderAgreement(EventSchemaRegistry registry)
        => new BusinessProviderProviderGovernanceProviderAgreementSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessProviderProviderScopeProviderAvailability(EventSchemaRegistry registry)
        => new BusinessProviderProviderScopeProviderAvailabilitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessProviderProviderScopeProviderCoverage(EventSchemaRegistry registry)
        => new BusinessProviderProviderScopeProviderCoverageSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — system-policy context (6 BCs).
    /// </summary>
    public static void RegisterControlSystemPolicyPolicyDefinition(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemPolicyPolicyPackage(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyPackageSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemPolicyPolicyEvaluation(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyEvaluationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemPolicyPolicyEnforcement(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyEnforcementSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemPolicyPolicyDecision(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyDecisionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemPolicyPolicyAudit(EventSchemaRegistry registry)
        => new ControlSystemPolicyPolicyAuditSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — access-control context (6 BCs).
    /// </summary>
    public static void RegisterControlAccessControlAccessPolicy(EventSchemaRegistry registry)
        => new ControlAccessControlAccessPolicySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAccessControlAuthorization(EventSchemaRegistry registry)
        => new ControlAccessControlAuthorizationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAccessControlIdentity(EventSchemaRegistry registry)
        => new ControlAccessControlIdentitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAccessControlPermission(EventSchemaRegistry registry)
        => new ControlAccessControlPermissionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAccessControlPrincipal(EventSchemaRegistry registry)
        => new ControlAccessControlPrincipalSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAccessControlRole(EventSchemaRegistry registry)
        => new ControlAccessControlRoleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — configuration context (5 BCs).
    /// </summary>
    public static void RegisterControlConfigurationConfigurationAssignment(EventSchemaRegistry registry)
        => new ControlConfigurationConfigurationAssignmentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlConfigurationConfigurationDefinition(EventSchemaRegistry registry)
        => new ControlConfigurationConfigurationDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlConfigurationConfigurationResolution(EventSchemaRegistry registry)
        => new ControlConfigurationConfigurationResolutionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlConfigurationConfigurationScope(EventSchemaRegistry registry)
        => new ControlConfigurationConfigurationScopeSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlConfigurationConfigurationState(EventSchemaRegistry registry)
        => new ControlConfigurationConfigurationStateSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — audit context (5 BCs).
    /// </summary>
    public static void RegisterControlAuditAuditEvent(EventSchemaRegistry registry)
        => new ControlAuditAuditEventSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAuditAuditLog(EventSchemaRegistry registry)
        => new ControlAuditAuditLogSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAuditAuditQuery(EventSchemaRegistry registry)
        => new ControlAuditAuditQuerySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAuditAuditRecord(EventSchemaRegistry registry)
        => new ControlAuditAuditRecordSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlAuditAuditTrace(EventSchemaRegistry registry)
        => new ControlAuditAuditTraceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — observability context (5 BCs).
    /// </summary>
    public static void RegisterControlObservabilitySystemAlert(EventSchemaRegistry registry)
        => new ControlObservabilitySystemAlertSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlObservabilitySystemHealth(EventSchemaRegistry registry)
        => new ControlObservabilitySystemHealthSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlObservabilitySystemMetric(EventSchemaRegistry registry)
        => new ControlObservabilitySystemMetricSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlObservabilitySystemSignal(EventSchemaRegistry registry)
        => new ControlObservabilitySystemSignalSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlObservabilitySystemTrace(EventSchemaRegistry registry)
        => new ControlObservabilitySystemTraceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — scheduling context (3 BCs).
    /// </summary>
    public static void RegisterControlSchedulingExecutionControl(EventSchemaRegistry registry)
        => new ControlSchedulingExecutionControlSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSchedulingScheduleControl(EventSchemaRegistry registry)
        => new ControlSchedulingScheduleControlSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSchedulingSystemJob(EventSchemaRegistry registry)
        => new ControlSchedulingSystemJobSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Control-system vertical — system-reconciliation context (5 BCs).
    /// </summary>
    public static void RegisterControlSystemReconciliationConsistencyCheck(EventSchemaRegistry registry)
        => new ControlSystemReconciliationConsistencyCheckSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemReconciliationDiscrepancyDetection(EventSchemaRegistry registry)
        => new ControlSystemReconciliationDiscrepancyDetectionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemReconciliationDiscrepancyResolution(EventSchemaRegistry registry)
        => new ControlSystemReconciliationDiscrepancyResolutionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemReconciliationReconciliationRun(EventSchemaRegistry registry)
        => new ControlSystemReconciliationReconciliationRunSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterControlSystemReconciliationSystemVerification(EventSchemaRegistry registry)
        => new ControlSystemReconciliationSystemVerificationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Trust-system vertical — identity context E1→Ex delivery (11 BCs).
    /// Phase 2.8 — all trust-system aggregates D2-promoted and schema-registered.
    /// </summary>
    public static void RegisterTrustIdentityIdentity(EventSchemaRegistry registry)
        => new TrustIdentityIdentitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityCredential(EventSchemaRegistry registry)
        => new TrustIdentityCredentialSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityVerification(EventSchemaRegistry registry)
        => new TrustIdentityVerificationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityProfile(EventSchemaRegistry registry)
        => new TrustIdentityProfileSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityRegistry(EventSchemaRegistry registry)
        => new TrustIdentityRegistrySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityTrust(EventSchemaRegistry registry)
        => new TrustIdentityTrustSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityServiceIdentity(EventSchemaRegistry registry)
        => new TrustIdentityServiceIdentitySchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityConsent(EventSchemaRegistry registry)
        => new TrustIdentityConsentSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityDevice(EventSchemaRegistry registry)
        => new TrustIdentityDeviceSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityIdentityGraph(EventSchemaRegistry registry)
        => new TrustIdentityIdentityGraphSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustIdentityFederation(EventSchemaRegistry registry)
        => new TrustIdentityFederationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Trust-system vertical — access context E1→Ex delivery (6 BCs).
    /// </summary>
    public static void RegisterTrustAccessSession(EventSchemaRegistry registry)
        => new TrustAccessSessionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustAccessRole(EventSchemaRegistry registry)
        => new TrustAccessRoleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustAccessGrant(EventSchemaRegistry registry)
        => new TrustAccessGrantSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustAccessRequest(EventSchemaRegistry registry)
        => new TrustAccessRequestSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustAccessPermission(EventSchemaRegistry registry)
        => new TrustAccessPermissionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterTrustAccessAuthorization(EventSchemaRegistry registry)
        => new TrustAccessAuthorizationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Business-system vertical — offering context E1→Ex delivery (7 BCs across 2 groups).
    /// </summary>
    public static void RegisterBusinessOfferingCatalogCoreBundle(EventSchemaRegistry registry)
        => new BusinessOfferingCatalogCoreBundleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCatalogCoreCatalog(EventSchemaRegistry registry)
        => new BusinessOfferingCatalogCoreCatalogSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCatalogCoreProduct(EventSchemaRegistry registry)
        => new BusinessOfferingCatalogCoreProductSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCatalogCoreServiceOffering(EventSchemaRegistry registry)
        => new BusinessOfferingCatalogCoreServiceOfferingSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCommercialShapeConfiguration(EventSchemaRegistry registry)
        => new BusinessOfferingCommercialShapeConfigurationSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCommercialShapePackage(EventSchemaRegistry registry)
        => new BusinessOfferingCommercialShapePackageSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterBusinessOfferingCommercialShapePlan(EventSchemaRegistry registry)
        => new BusinessOfferingCommercialShapePlanSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Platform-system vertical — command context schema bindings (3 event-bearing BCs).
    /// command-envelope is a pure structural VO domain and has no events.
    /// </summary>
    public static void RegisterPlatformCommandCommandDefinition(EventSchemaRegistry registry)
        => new PlatformCommandDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformCommandCommandMetadata(EventSchemaRegistry registry)
        => new PlatformCommandMetadataSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformCommandCommandRouting(EventSchemaRegistry registry)
        => new PlatformCommandRoutingSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Platform-system vertical — event context schema bindings (4 event-bearing BCs).
    /// event-envelope is a pure structural VO domain and has no events.
    /// </summary>
    public static void RegisterPlatformEventEventDefinition(EventSchemaRegistry registry)
        => new PlatformEventDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEventEventMetadata(EventSchemaRegistry registry)
        => new PlatformEventMetadataSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEventEventSchema(EventSchemaRegistry registry)
        => new PlatformEventSchemaSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEventEventStream(EventSchemaRegistry registry)
        => new PlatformEventStreamSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Platform-system vertical — envelope context schema bindings (4 BCs).
    /// </summary>
    public static void RegisterPlatformEnvelopeHeader(EventSchemaRegistry registry)
        => new PlatformEnvelopeHeaderSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEnvelopeMessageEnvelope(EventSchemaRegistry registry)
        => new PlatformMessageEnvelopeSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEnvelopeMetadata(EventSchemaRegistry registry)
        => new PlatformEnvelopeMetadataSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformEnvelopePayload(EventSchemaRegistry registry)
        => new PlatformPayloadSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Platform-system vertical — routing context schema bindings (4 BCs).
    /// </summary>
    public static void RegisterPlatformRoutingDispatchRule(EventSchemaRegistry registry)
        => new PlatformDispatchRuleSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformRoutingRouteDefinition(EventSchemaRegistry registry)
        => new PlatformRouteDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformRoutingRouteDescriptor(EventSchemaRegistry registry)
        => new PlatformRouteDescriptorSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformRoutingRouteResolution(EventSchemaRegistry registry)
        => new PlatformRouteResolutionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    /// <summary>
    /// Platform-system vertical — schema context schema bindings (4 BCs).
    /// </summary>
    public static void RegisterPlatformSchemaContract(EventSchemaRegistry registry)
        => new PlatformContractSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformSchemaSchemaDefinition(EventSchemaRegistry registry)
        => new PlatformSchemaDefinitionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformSchemaSerializationFormat(EventSchemaRegistry registry)
        => new PlatformSerializationFormatSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterPlatformSchemaVersioningRule(EventSchemaRegistry registry)
        => new PlatformVersioningRuleSchemaModule().Register(new EventSchemaRegistrySink(registry));
}
