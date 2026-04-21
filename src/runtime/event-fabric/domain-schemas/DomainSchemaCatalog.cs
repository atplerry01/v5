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
    /// Business-system vertical — agreement/commitment/contract BC schema bindings.
    /// Ex pilot: first full E1→Ex delivery for the business classification.
    /// </summary>
    public static void RegisterBusinessAgreementCommitmentContract(EventSchemaRegistry registry)
        => new BusinessAgreementCommitmentContractSchemaModule().Register(new EventSchemaRegistrySink(registry));
}
