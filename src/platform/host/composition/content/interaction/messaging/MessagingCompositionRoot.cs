using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Content.Interaction.Messaging.Application;
using Whycespace.Platform.Host.Composition.Content.Interaction.Messaging.Projection;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Content.Interaction.Messaging;

/// <summary>
/// Phase 1 composition root for content-system/interaction/messaging. Wires
/// T2E command handlers, event schemas, and Kafka-backed projections for the
/// canonical topic whyce.content.interaction.messaging.events.
/// </summary>
public sealed class MessagingCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessagingApplication();
        services.AddMessagingProjection(configuration);
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterContentInteractionMessaging(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        MessagingProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        MessagingApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
    }
}
