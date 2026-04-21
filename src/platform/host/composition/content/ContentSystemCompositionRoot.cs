using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Document.Application;
using Whycespace.Projections.Content.Document.CoreObject.Document;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Content;

/// <summary>
/// Composition root for the content classification.
///
/// Current scope (2026-04-21): exemplar delivery of one BC —
/// <c>content/document/core-object/document</c>. Full 26-BC expansion is
/// tracked in the E1→EX content-system delivery matrix under
/// <c>claude/audits/sweeps/</c>. Adding a new content BC here means:
///   1. Register its application module under RegisterServices + RegisterEngines.
///   2. Register its projection store + handler under RegisterProjections.
///   3. Register its schema module in DomainSchemaCatalog.
/// </summary>
public sealed class ContentSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDocumentApplication();

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentReadModel>(
                    "projection_content_document_core_object_document",
                    "document_read_model",
                    "Document"));

        services.AddSingleton(sp => new DocumentProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentReadModel>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectDocument(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var documentHandler = provider.GetRequiredService<DocumentProjectionHandler>();
        projection.Register("DocumentCreatedEvent", documentHandler);
        projection.Register("DocumentMetadataUpdatedEvent", documentHandler);
        projection.Register("DocumentVersionAttachedEvent", documentHandler);
        projection.Register("DocumentActivatedEvent", documentHandler);
        projection.Register("DocumentArchivedEvent", documentHandler);
        projection.Register("DocumentRestoredEvent", documentHandler);
        projection.Register("DocumentSupersededEvent", documentHandler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        DocumentApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows required for content/document/core-object/document
        // — lifecycle is single-shot state transitions, no multi-step saga.
    }
}
