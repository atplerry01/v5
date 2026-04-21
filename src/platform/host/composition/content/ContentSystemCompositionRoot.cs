using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Bundle.Application;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Document.Application;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.File.Application;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Record.Application;
using Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Template.Application;
using Whycespace.Platform.Host.Composition.Content.Document.Descriptor.Metadata.Application;
using Whycespace.Platform.Host.Composition.Content.Document.Governance.Retention.Application;
using Whycespace.Platform.Host.Composition.Content.Document.Intake.Upload.Application;
using Whycespace.Platform.Host.Composition.Content.Document.LifecycleChange.Processing.Application;
using Whycespace.Platform.Host.Composition.Content.Document.LifecycleChange.Version.Application;
using Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Asset.Application;
using Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Subtitle.Application;
using Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Transcript.Application;
using Whycespace.Platform.Host.Composition.Content.Media.Descriptor.Metadata.Application;
using Whycespace.Platform.Host.Composition.Content.Media.Intake.Ingest.Application;
using Whycespace.Platform.Host.Composition.Content.Media.LifecycleChange.Version.Application;
using Whycespace.Platform.Host.Composition.Content.Media.TechnicalProcessing.Processing.Application;
using Whycespace.Projections.Content.Document.CoreObject.Bundle;
using Whycespace.Projections.Content.Document.CoreObject.Document;
using Whycespace.Projections.Content.Document.CoreObject.File;
using Whycespace.Projections.Content.Document.CoreObject.Record;
using Whycespace.Projections.Content.Document.CoreObject.Template;
using Whycespace.Projections.Content.Document.Descriptor.Metadata;
using Whycespace.Projections.Content.Document.Governance.Retention;
using Whycespace.Projections.Content.Document.Intake.Upload;
using Whycespace.Projections.Content.Document.LifecycleChange.Processing;
using Whycespace.Projections.Content.Document.LifecycleChange.Version;
using Whycespace.Projections.Content.Media.CoreObject.Asset;
using Whycespace.Projections.Content.Media.CoreObject.Subtitle;
using Whycespace.Projections.Content.Media.CoreObject.Transcript;
using Whycespace.Projections.Content.Media.Descriptor.Metadata;
using Whycespace.Projections.Content.Media.Intake.Ingest;
using Whycespace.Projections.Content.Media.LifecycleChange.Version;
using Whycespace.Projections.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Content;

/// <summary>
/// Composition root for the content classification.
///
/// E1→EX delivery (2026-04-21): 10 populated document-context BCs wired
/// end-to-end. Each BC gets:
///   - Application module (engine handlers + engine registry)
///   - Projection store + projection handler
///   - Schema module registration
///   - Projection event-type registrations (one per event)
/// </summary>
public sealed class ContentSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── Application modules (engine handlers) ────────────────
        services.AddDocumentApplication();
        services.AddDocumentBundleApplication();
        services.AddDocumentFileApplication();
        services.AddDocumentRecordApplication();
        services.AddDocumentTemplateApplication();
        services.AddDocumentMetadataApplication();
        services.AddRetentionApplication();
        services.AddDocumentUploadApplication();
        services.AddDocumentProcessingApplication();
        services.AddDocumentVersionApplication();

        // media-context application modules
        services.AddAssetApplication();
        services.AddSubtitleApplication();
        services.AddTranscriptApplication();
        services.AddMediaMetadataApplication();
        services.AddMediaIngestApplication();
        services.AddMediaVersionApplication();
        services.AddMediaProcessingApplication();

        // ── Projection stores ────────────────────────────────────
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentReadModel>("projection_content_document_core_object_document", "document_read_model", "Document"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentBundleReadModel>("projection_content_document_core_object_bundle", "document_bundle_read_model", "DocumentBundle"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentFileReadModel>("projection_content_document_core_object_file", "document_file_read_model", "DocumentFile"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentRecordReadModel>("projection_content_document_core_object_record", "document_record_read_model", "DocumentRecord"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentTemplateReadModel>("projection_content_document_core_object_template", "document_template_read_model", "DocumentTemplate"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentMetadataReadModel>("projection_content_document_descriptor_metadata", "document_metadata_read_model", "DocumentMetadata"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<RetentionReadModel>("projection_content_document_governance_retention", "retention_read_model", "Retention"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentUploadReadModel>("projection_content_document_intake_upload", "document_upload_read_model", "DocumentUpload"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentProcessingReadModel>("projection_content_document_lifecycle_change_processing", "document_processing_read_model", "DocumentProcessing"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DocumentVersionReadModel>("projection_content_document_lifecycle_change_version", "document_version_read_model", "DocumentVersion"));

        // media context projection stores
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AssetReadModel>("projection_content_media_core_object_asset", "asset_read_model", "Asset"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SubtitleReadModel>("projection_content_media_core_object_subtitle", "subtitle_read_model", "Subtitle"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TranscriptReadModel>("projection_content_media_core_object_transcript", "transcript_read_model", "Transcript"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<MediaMetadataReadModel>("projection_content_media_descriptor_metadata", "media_metadata_read_model", "MediaMetadata"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<MediaIngestReadModel>("projection_content_media_intake_ingest", "media_ingest_read_model", "MediaIngest"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<MediaVersionReadModel>("projection_content_media_lifecycle_change_version", "media_version_read_model", "MediaVersion"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<MediaProcessingReadModel>("projection_content_media_technical_processing_processing", "media_processing_read_model", "MediaProcessing"));

        // ── Projection handlers ──────────────────────────────────
        services.AddSingleton(sp => new DocumentProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentReadModel>>()));
        services.AddSingleton(sp => new DocumentBundleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentBundleReadModel>>()));
        services.AddSingleton(sp => new DocumentFileProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentFileReadModel>>()));
        services.AddSingleton(sp => new DocumentRecordProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentRecordReadModel>>()));
        services.AddSingleton(sp => new DocumentTemplateProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentTemplateReadModel>>()));
        services.AddSingleton(sp => new DocumentMetadataProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentMetadataReadModel>>()));
        services.AddSingleton(sp => new RetentionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RetentionReadModel>>()));
        services.AddSingleton(sp => new DocumentUploadProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentUploadReadModel>>()));
        services.AddSingleton(sp => new DocumentProcessingProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentProcessingReadModel>>()));
        services.AddSingleton(sp => new DocumentVersionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DocumentVersionReadModel>>()));

        // media context projection handlers
        services.AddSingleton(sp => new AssetProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AssetReadModel>>()));
        services.AddSingleton(sp => new SubtitleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SubtitleReadModel>>()));
        services.AddSingleton(sp => new TranscriptProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TranscriptReadModel>>()));
        services.AddSingleton(sp => new MediaMetadataProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MediaMetadataReadModel>>()));
        services.AddSingleton(sp => new MediaIngestProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MediaIngestReadModel>>()));
        services.AddSingleton(sp => new MediaVersionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MediaVersionReadModel>>()));
        services.AddSingleton(sp => new MediaProcessingProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MediaProcessingReadModel>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectDocument(schema);
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectBundle(schema);
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectFile(schema);
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectRecord(schema);
        DomainSchemaCatalog.RegisterContentDocumentCoreObjectTemplate(schema);
        DomainSchemaCatalog.RegisterContentDocumentDescriptorMetadata(schema);
        DomainSchemaCatalog.RegisterContentDocumentGovernanceRetention(schema);
        DomainSchemaCatalog.RegisterContentDocumentIntakeUpload(schema);
        DomainSchemaCatalog.RegisterContentDocumentLifecycleChangeProcessing(schema);
        DomainSchemaCatalog.RegisterContentDocumentLifecycleChangeVersion(schema);
        DomainSchemaCatalog.RegisterContentMediaCoreObjectAsset(schema);
        DomainSchemaCatalog.RegisterContentMediaCoreObjectSubtitle(schema);
        DomainSchemaCatalog.RegisterContentMediaCoreObjectTranscript(schema);
        DomainSchemaCatalog.RegisterContentMediaDescriptorMetadata(schema);
        DomainSchemaCatalog.RegisterContentMediaIntakeIngest(schema);
        DomainSchemaCatalog.RegisterContentMediaLifecycleChangeVersion(schema);
        DomainSchemaCatalog.RegisterContentMediaTechnicalProcessingProcessing(schema);
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

        var bundleHandler = provider.GetRequiredService<DocumentBundleProjectionHandler>();
        projection.Register("DocumentBundleCreatedEvent", bundleHandler);
        projection.Register("DocumentBundleRenamedEvent", bundleHandler);
        projection.Register("DocumentBundleMemberAddedEvent", bundleHandler);
        projection.Register("DocumentBundleMemberRemovedEvent", bundleHandler);
        projection.Register("DocumentBundleFinalizedEvent", bundleHandler);
        projection.Register("DocumentBundleArchivedEvent", bundleHandler);

        var fileHandler = provider.GetRequiredService<DocumentFileProjectionHandler>();
        projection.Register("DocumentFileRegisteredEvent", fileHandler);
        projection.Register("DocumentFileIntegrityVerifiedEvent", fileHandler);
        projection.Register("DocumentFileSupersededEvent", fileHandler);
        projection.Register("DocumentFileArchivedEvent", fileHandler);

        var recordHandler = provider.GetRequiredService<DocumentRecordProjectionHandler>();
        projection.Register("DocumentRecordCreatedEvent", recordHandler);
        projection.Register("DocumentRecordLockedEvent", recordHandler);
        projection.Register("DocumentRecordUnlockedEvent", recordHandler);
        projection.Register("DocumentRecordClosedEvent", recordHandler);
        projection.Register("DocumentRecordArchivedEvent", recordHandler);

        var templateHandler = provider.GetRequiredService<DocumentTemplateProjectionHandler>();
        projection.Register("DocumentTemplateCreatedEvent", templateHandler);
        projection.Register("DocumentTemplateUpdatedEvent", templateHandler);
        projection.Register("DocumentTemplateActivatedEvent", templateHandler);
        projection.Register("DocumentTemplateDeprecatedEvent", templateHandler);
        projection.Register("DocumentTemplateArchivedEvent", templateHandler);

        var metadataHandler = provider.GetRequiredService<DocumentMetadataProjectionHandler>();
        projection.Register("DocumentMetadataCreatedEvent", metadataHandler);
        projection.Register("DocumentMetadataEntryAddedEvent", metadataHandler);
        projection.Register("DocumentMetadataEntryUpdatedEvent", metadataHandler);
        projection.Register("DocumentMetadataEntryRemovedEvent", metadataHandler);
        projection.Register("DocumentMetadataFinalizedEvent", metadataHandler);

        var retentionHandler = provider.GetRequiredService<RetentionProjectionHandler>();
        projection.Register("RetentionAppliedEvent", retentionHandler);
        projection.Register("RetentionHoldPlacedEvent", retentionHandler);
        projection.Register("RetentionReleasedEvent", retentionHandler);
        projection.Register("RetentionExpiredEvent", retentionHandler);
        projection.Register("RetentionMarkedEligibleForDestructionEvent", retentionHandler);
        projection.Register("RetentionArchivedEvent", retentionHandler);

        var uploadHandler = provider.GetRequiredService<DocumentUploadProjectionHandler>();
        projection.Register("DocumentUploadRequestedEvent", uploadHandler);
        projection.Register("DocumentUploadAcceptedEvent", uploadHandler);
        projection.Register("DocumentUploadProcessingStartedEvent", uploadHandler);
        projection.Register("DocumentUploadCompletedEvent", uploadHandler);
        projection.Register("DocumentUploadFailedEvent", uploadHandler);
        projection.Register("DocumentUploadCancelledEvent", uploadHandler);

        var processingHandler = provider.GetRequiredService<DocumentProcessingProjectionHandler>();
        projection.Register("DocumentProcessingRequestedEvent", processingHandler);
        projection.Register("DocumentProcessingStartedEvent", processingHandler);
        projection.Register("DocumentProcessingCompletedEvent", processingHandler);
        projection.Register("DocumentProcessingFailedEvent", processingHandler);
        projection.Register("DocumentProcessingCancelledEvent", processingHandler);

        var versionHandler = provider.GetRequiredService<DocumentVersionProjectionHandler>();
        projection.Register("DocumentVersionCreatedEvent", versionHandler);
        projection.Register("DocumentVersionActivatedEvent", versionHandler);
        projection.Register("DocumentVersionSupersededEvent", versionHandler);
        projection.Register("DocumentVersionWithdrawnEvent", versionHandler);

        var assetHandler = provider.GetRequiredService<AssetProjectionHandler>();
        projection.Register("AssetCreatedEvent", assetHandler);
        projection.Register("AssetRenamedEvent", assetHandler);
        projection.Register("AssetReclassifiedEvent", assetHandler);
        projection.Register("AssetActivatedEvent", assetHandler);
        projection.Register("AssetRetiredEvent", assetHandler);
        projection.Register("AssetKindAssignedEvent", assetHandler);

        var subtitleHandler = provider.GetRequiredService<SubtitleProjectionHandler>();
        projection.Register("SubtitleCreatedEvent", subtitleHandler);
        projection.Register("SubtitleUpdatedEvent", subtitleHandler);
        projection.Register("SubtitleFinalizedEvent", subtitleHandler);
        projection.Register("SubtitleArchivedEvent", subtitleHandler);

        var transcriptHandler = provider.GetRequiredService<TranscriptProjectionHandler>();
        projection.Register("TranscriptCreatedEvent", transcriptHandler);
        projection.Register("TranscriptUpdatedEvent", transcriptHandler);
        projection.Register("TranscriptFinalizedEvent", transcriptHandler);
        projection.Register("TranscriptArchivedEvent", transcriptHandler);

        var mediaMetadataHandler = provider.GetRequiredService<MediaMetadataProjectionHandler>();
        projection.Register("MediaMetadataCreatedEvent", mediaMetadataHandler);
        projection.Register("MediaMetadataEntryAddedEvent", mediaMetadataHandler);
        projection.Register("MediaMetadataEntryUpdatedEvent", mediaMetadataHandler);
        projection.Register("MediaMetadataEntryRemovedEvent", mediaMetadataHandler);
        projection.Register("MediaMetadataFinalizedEvent", mediaMetadataHandler);

        var mediaIngestHandler = provider.GetRequiredService<MediaIngestProjectionHandler>();
        projection.Register("MediaIngestRequestedEvent", mediaIngestHandler);
        projection.Register("MediaIngestAcceptedEvent", mediaIngestHandler);
        projection.Register("MediaIngestProcessingStartedEvent", mediaIngestHandler);
        projection.Register("MediaIngestCompletedEvent", mediaIngestHandler);
        projection.Register("MediaIngestFailedEvent", mediaIngestHandler);
        projection.Register("MediaIngestCancelledEvent", mediaIngestHandler);

        var mediaVersionHandler = provider.GetRequiredService<MediaVersionProjectionHandler>();
        projection.Register("MediaVersionCreatedEvent", mediaVersionHandler);
        projection.Register("MediaVersionActivatedEvent", mediaVersionHandler);
        projection.Register("MediaVersionSupersededEvent", mediaVersionHandler);
        projection.Register("MediaVersionWithdrawnEvent", mediaVersionHandler);

        var mediaProcessingHandler = provider.GetRequiredService<MediaProcessingProjectionHandler>();
        projection.Register("MediaProcessingRequestedEvent", mediaProcessingHandler);
        projection.Register("MediaProcessingStartedEvent", mediaProcessingHandler);
        projection.Register("MediaProcessingCompletedEvent", mediaProcessingHandler);
        projection.Register("MediaProcessingFailedEvent", mediaProcessingHandler);
        projection.Register("MediaProcessingCancelledEvent", mediaProcessingHandler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        DocumentApplicationModule.RegisterEngines(engine);
        DocumentBundleApplicationModule.RegisterEngines(engine);
        DocumentFileApplicationModule.RegisterEngines(engine);
        DocumentRecordApplicationModule.RegisterEngines(engine);
        DocumentTemplateApplicationModule.RegisterEngines(engine);
        DocumentMetadataApplicationModule.RegisterEngines(engine);
        RetentionApplicationModule.RegisterEngines(engine);
        DocumentUploadApplicationModule.RegisterEngines(engine);
        DocumentProcessingApplicationModule.RegisterEngines(engine);
        DocumentVersionApplicationModule.RegisterEngines(engine);
        AssetApplicationModule.RegisterEngines(engine);
        SubtitleApplicationModule.RegisterEngines(engine);
        TranscriptApplicationModule.RegisterEngines(engine);
        MediaMetadataApplicationModule.RegisterEngines(engine);
        MediaIngestApplicationModule.RegisterEngines(engine);
        MediaVersionApplicationModule.RegisterEngines(engine);
        MediaProcessingApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows required for the document context at D2 —
        // all 10 BCs are single-shot transitions / factory-inits only.
    }
}
