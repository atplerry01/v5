using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Host.Composition;

/// <summary>
/// Per-domain bootstrap contract. Owns ALL wiring for a single classification/context/domain
/// so that Program.cs is free of domain-specific knowledge.
///
/// Lifecycle:
///   1. Program.cs constructs the static module list and calls RegisterServices on each
///      (passing IServiceCollection and IConfiguration) — happens before builder.Build().
///   2. Each module is also DI-registered as IDomainBootstrapModule so factories below
///      can resolve all modules via sp.GetServices&lt;IDomainBootstrapModule&gt;().
///   3. The factories for IEngineRegistry, IWorkflowRegistry, EventSchemaRegistry, and
///      ProjectionRegistry iterate the modules and call the corresponding Register* methods
///      INSIDE the factory closure, BEFORE calling Lock() — preserving the
///      lock-after-build immutability guarantee.
///
/// Schema CLR-type extensions and the generic Kafka consumer are part of this contract.
/// Phase 1.5 §5.1.2 BPV-D01 further moved typed-domain schema binding behind the
/// runtime-side DomainSchemaCatalog seam; host modules now dispatch via that seam
/// rather than typing domain events directly.
/// </summary>
public interface IDomainBootstrapModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void RegisterSchema(EventSchemaRegistry schema);

    void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection);

    void RegisterEngines(IEngineRegistry engine);

    void RegisterWorkflows(IWorkflowRegistry workflow);

    /// <summary>
    /// Registers CLR types that may appear as opaque <c>object?</c> Payload or
    /// step Output values on this domain's workflow lifecycle events. Without
    /// registration the type round-trips as <c>JsonElement</c> on Postgres-backed
    /// replay (current behavior). With registration the deserializer rehydrates
    /// it back into the original CLR type. Default no-op for back-compat.
    /// </summary>
    void RegisterPayloadTypes(IPayloadTypeRegistry registry) { }
}
