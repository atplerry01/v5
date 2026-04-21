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
    public static void RegisterOperationalSandboxTodo(EventSchemaRegistry registry)
        => new TodoSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterConstitutionalPolicyDecision(EventSchemaRegistry registry)
        => new PolicyDecisionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterOrchestrationWorkflowExecution(EventSchemaRegistry registry)
        => new WorkflowExecutionSchemaModule().Register(new EventSchemaRegistrySink(registry));

    public static void RegisterOperationalSandboxKanban(EventSchemaRegistry registry)
        => new KanbanSchemaModule().Register(new EventSchemaRegistrySink(registry));

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
}
