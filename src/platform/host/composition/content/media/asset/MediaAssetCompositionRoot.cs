using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Content.Media.Asset.Application;
using Whycespace.Platform.Host.Composition.Content.Media.Asset.Projection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Content.Media.Asset;

/// <summary>
/// Phase 1 composition root for content-system/media/asset. Wires T2E command
/// handlers, event schemas, and Kafka-backed projections for the canonical
/// topic whyce.content.media.asset.events.
/// </summary>
public sealed class MediaAssetCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediaAssetApplication();
        services.AddMediaAssetProjection(configuration);
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterContentMediaAsset(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        MediaAssetProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        MediaAssetApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
    }
}
