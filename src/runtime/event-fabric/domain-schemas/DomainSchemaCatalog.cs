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
}
