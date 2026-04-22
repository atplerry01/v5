using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Constitutional.Chain.Application;
using Whycespace.Platform.Host.Composition.Constitutional.Chain.Projection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Constitutional.Chain;

/// <summary>
/// Phase 2.9 — bootstrap module for the constitutional-system chain context.
/// Wires anchor-record, evidence-record, and ledger BCs: engine handlers,
/// event schema bindings, projection handlers, and Kafka consumers.
/// </summary>
public sealed class ConstitutionalChainBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddChainApplication();
        services.AddChainProjection(configuration);
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterConstitutionalChainAnchorRecord(schema);
        DomainSchemaCatalog.RegisterConstitutionalChainEvidenceRecord(schema);
        DomainSchemaCatalog.RegisterConstitutionalChainLedger(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        ChainProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        ChainApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No workflows for chain BCs.
    }
}
