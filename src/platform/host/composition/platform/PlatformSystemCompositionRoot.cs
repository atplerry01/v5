using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Platform.Command.CommandDefinition.Application;
using Whycespace.Platform.Host.Composition.Platform.Command.CommandMetadata.Application;
using Whycespace.Platform.Host.Composition.Platform.Command.CommandRouting.Application;
using Whycespace.Platform.Host.Composition.Platform.Envelope.Header.Application;
using Whycespace.Platform.Host.Composition.Platform.Envelope.MessageEnvelope.Application;
using Whycespace.Platform.Host.Composition.Platform.Envelope.Metadata.Application;
using Whycespace.Platform.Host.Composition.Platform.Envelope.Payload.Application;
using Whycespace.Platform.Host.Composition.Platform.Event.EventDefinition.Application;
using Whycespace.Platform.Host.Composition.Platform.Event.EventMetadata.Application;
using Whycespace.Platform.Host.Composition.Platform.Event.EventSchema.Application;
using Whycespace.Platform.Host.Composition.Platform.Event.EventStream.Application;
using Whycespace.Platform.Host.Composition.Platform.Routing.DispatchRule.Application;
using Whycespace.Platform.Host.Composition.Platform.Routing.RouteDefinition.Application;
using Whycespace.Platform.Host.Composition.Platform.Routing.RouteDescriptor.Application;
using Whycespace.Platform.Host.Composition.Platform.Routing.RouteResolution.Application;
using Whycespace.Platform.Host.Composition.Platform.Schema.Contract.Application;
using Whycespace.Platform.Host.Composition.Platform.Schema.SchemaDefinition.Application;
using Whycespace.Platform.Host.Composition.Platform.Schema.Serialization.Application;
using Whycespace.Platform.Host.Composition.Platform.Schema.Versioning.Application;
using Whycespace.Projections.Platform.Command.CommandDefinition;
using Whycespace.Projections.Platform.Command.CommandMetadata;
using Whycespace.Projections.Platform.Envelope.MessageEnvelope;
using Whycespace.Projections.Platform.Event.EventDefinition;
using Whycespace.Projections.Platform.Event.EventMetadata;
using Whycespace.Projections.Platform.Routing.DispatchRule;
using Whycespace.Projections.Platform.Routing.RouteDefinition;
using Whycespace.Projections.Platform.Routing.RouteResolution;
using Whycespace.Projections.Platform.Schema.Contract;
using Whycespace.Projections.Platform.Schema.SchemaDefinition;
using Whycespace.Projections.Platform.Schema.Serialization;
using Whycespace.Projections.Platform.Schema.Versioning;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Platform;

/// <summary>
/// Composition root for the platform classification.
///
/// E1→EX delivery: 21 BCs across 5 contexts (command, event, envelope, routing, schema).
/// command-envelope and event-envelope are pure structural VO domains with no events or handlers.
/// 19 BCs have engine handlers; 12 BCs have projection stores.
/// </summary>
public sealed class PlatformSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── Application modules (engine handlers) ─────────────────────
        services.AddCommandDefinitionApplication();
        services.AddCommandMetadataApplication();
        services.AddCommandRoutingApplication();

        services.AddEventDefinitionApplication();
        services.AddEventMetadataApplication();
        services.AddEventSchemaApplication();
        services.AddEventStreamApplication();

        services.AddHeaderSchemaApplication();
        services.AddMessageEnvelopeApplication();
        services.AddMessageMetadataSchemaApplication();
        services.AddPayloadSchemaApplication();

        services.AddDispatchRuleApplication();
        services.AddRouteDefinitionApplication();
        services.AddRouteDescriptorApplication();
        services.AddRouteResolutionApplication();

        services.AddContractApplication();
        services.AddSchemaDefinitionApplication();
        services.AddSerializationFormatApplication();
        services.AddVersioningRuleApplication();

        // ── Projection stores (12 BCs with observable state) ─────────
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<CommandDefinitionReadModel>("projection_platform_command_command_definition", "command_definition_read_model", "CommandDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<CommandMetadataReadModel>("projection_platform_command_command_metadata", "command_metadata_read_model", "CommandMetadata"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<EventDefinitionReadModel>("projection_platform_event_event_definition", "event_definition_read_model", "EventDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<EventMetadataReadModel>("projection_platform_event_event_metadata", "event_metadata_read_model", "EventMetadata"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<MessageEnvelopeReadModel>("projection_platform_envelope_message_envelope", "message_envelope_read_model", "MessageEnvelope"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DispatchRuleReadModel>("projection_platform_routing_dispatch_rule", "dispatch_rule_read_model", "DispatchRule"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<RouteDefinitionReadModel>("projection_platform_routing_route_definition", "route_definition_read_model", "RouteDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<RouteResolutionReadModel>("projection_platform_routing_route_resolution", "route_resolution_read_model", "RouteResolution"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SchemaDefinitionReadModel>("projection_platform_schema_schema_definition", "schema_definition_read_model", "SchemaDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ContractReadModel>("projection_platform_schema_contract", "contract_read_model", "Contract"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SerializationFormatReadModel>("projection_platform_schema_serialization_format", "serialization_format_read_model", "SerializationFormat"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<VersioningRuleReadModel>("projection_platform_schema_versioning_rule", "versioning_rule_read_model", "VersioningRule"));

        // ── Projection handlers ───────────────────────────────────────
        services.AddSingleton(sp => new CommandDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CommandDefinitionReadModel>>()));
        services.AddSingleton(sp => new CommandMetadataProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CommandMetadataReadModel>>()));
        services.AddSingleton(sp => new EventDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EventDefinitionReadModel>>()));
        services.AddSingleton(sp => new EventMetadataProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EventMetadataReadModel>>()));
        services.AddSingleton(sp => new MessageEnvelopeProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MessageEnvelopeReadModel>>()));
        services.AddSingleton(sp => new DispatchRuleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DispatchRuleReadModel>>()));
        services.AddSingleton(sp => new RouteDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RouteDefinitionReadModel>>()));
        services.AddSingleton(sp => new RouteResolutionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RouteResolutionReadModel>>()));
        services.AddSingleton(sp => new SchemaDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SchemaDefinitionReadModel>>()));
        services.AddSingleton(sp => new ContractProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ContractReadModel>>()));
        services.AddSingleton(sp => new SerializationFormatProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SerializationFormatReadModel>>()));
        services.AddSingleton(sp => new VersioningRuleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<VersioningRuleReadModel>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterPlatformCommandCommandDefinition(schema);
        DomainSchemaCatalog.RegisterPlatformCommandCommandMetadata(schema);
        DomainSchemaCatalog.RegisterPlatformCommandCommandRouting(schema);

        DomainSchemaCatalog.RegisterPlatformEventEventDefinition(schema);
        DomainSchemaCatalog.RegisterPlatformEventEventMetadata(schema);
        DomainSchemaCatalog.RegisterPlatformEventEventSchema(schema);
        DomainSchemaCatalog.RegisterPlatformEventEventStream(schema);

        DomainSchemaCatalog.RegisterPlatformEnvelopeHeader(schema);
        DomainSchemaCatalog.RegisterPlatformEnvelopeMessageEnvelope(schema);
        DomainSchemaCatalog.RegisterPlatformEnvelopeMetadata(schema);
        DomainSchemaCatalog.RegisterPlatformEnvelopePayload(schema);

        DomainSchemaCatalog.RegisterPlatformRoutingDispatchRule(schema);
        DomainSchemaCatalog.RegisterPlatformRoutingRouteDefinition(schema);
        DomainSchemaCatalog.RegisterPlatformRoutingRouteDescriptor(schema);
        DomainSchemaCatalog.RegisterPlatformRoutingRouteResolution(schema);

        DomainSchemaCatalog.RegisterPlatformSchemaContract(schema);
        DomainSchemaCatalog.RegisterPlatformSchemaSchemaDefinition(schema);
        DomainSchemaCatalog.RegisterPlatformSchemaSerializationFormat(schema);
        DomainSchemaCatalog.RegisterPlatformSchemaVersioningRule(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var commandDefinitionHandler = provider.GetRequiredService<CommandDefinitionProjectionHandler>();
        projection.Register("CommandDefinedEvent", commandDefinitionHandler);
        projection.Register("CommandDeprecatedEvent", commandDefinitionHandler);

        var commandMetadataHandler = provider.GetRequiredService<CommandMetadataProjectionHandler>();
        projection.Register("CommandMetadataAttachedEvent", commandMetadataHandler);

        var eventDefinitionHandler = provider.GetRequiredService<EventDefinitionProjectionHandler>();
        projection.Register("EventDefinedEvent", eventDefinitionHandler);
        projection.Register("EventDefinitionDeprecatedEvent", eventDefinitionHandler);

        var eventMetadataHandler = provider.GetRequiredService<EventMetadataProjectionHandler>();
        projection.Register("EventMetadataAttachedEvent", eventMetadataHandler);

        var messageEnvelopeHandler = provider.GetRequiredService<MessageEnvelopeProjectionHandler>();
        projection.Register("MessageEnvelopeCreatedEvent", messageEnvelopeHandler);
        projection.Register("MessageEnvelopeDispatchedEvent", messageEnvelopeHandler);
        projection.Register("MessageEnvelopeAcknowledgedEvent", messageEnvelopeHandler);
        projection.Register("MessageEnvelopeRejectedEvent", messageEnvelopeHandler);

        var dispatchRuleHandler = provider.GetRequiredService<DispatchRuleProjectionHandler>();
        projection.Register("DispatchRuleRegisteredEvent", dispatchRuleHandler);
        projection.Register("DispatchRuleDeactivatedEvent", dispatchRuleHandler);

        var routeDefinitionHandler = provider.GetRequiredService<RouteDefinitionProjectionHandler>();
        projection.Register("RouteDefinitionRegisteredEvent", routeDefinitionHandler);
        projection.Register("RouteDefinitionDeactivatedEvent", routeDefinitionHandler);
        projection.Register("RouteDefinitionDeprecatedEvent", routeDefinitionHandler);

        var routeResolutionHandler = provider.GetRequiredService<RouteResolutionProjectionHandler>();
        projection.Register("RouteResolvedEvent", routeResolutionHandler);
        projection.Register("RouteResolutionFailedEvent", routeResolutionHandler);

        var schemaDefinitionHandler = provider.GetRequiredService<SchemaDefinitionProjectionHandler>();
        projection.Register("SchemaDefinitionDraftedEvent", schemaDefinitionHandler);
        projection.Register("SchemaDefinitionPublishedEvent", schemaDefinitionHandler);
        projection.Register("SchemaDefinitionDeprecatedEvent", schemaDefinitionHandler);

        var contractHandler = provider.GetRequiredService<ContractProjectionHandler>();
        projection.Register("ContractRegisteredEvent", contractHandler);
        projection.Register("ContractSubscriberAddedEvent", contractHandler);
        projection.Register("ContractDeprecatedEvent", contractHandler);

        var serializationFormatHandler = provider.GetRequiredService<SerializationFormatProjectionHandler>();
        projection.Register("SerializationFormatRegisteredEvent", serializationFormatHandler);
        projection.Register("SerializationFormatDeprecatedEvent", serializationFormatHandler);

        var versioningRuleHandler = provider.GetRequiredService<VersioningRuleProjectionHandler>();
        projection.Register("VersioningRuleRegisteredEvent", versioningRuleHandler);
        projection.Register("VersioningRuleVerdictIssuedEvent", versioningRuleHandler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        CommandDefinitionApplicationModule.RegisterEngines(engine);
        CommandMetadataApplicationModule.RegisterEngines(engine);
        CommandRoutingApplicationModule.RegisterEngines(engine);

        EventDefinitionApplicationModule.RegisterEngines(engine);
        EventMetadataApplicationModule.RegisterEngines(engine);
        EventSchemaApplicationModule.RegisterEngines(engine);
        EventStreamApplicationModule.RegisterEngines(engine);

        HeaderSchemaApplicationModule.RegisterEngines(engine);
        MessageEnvelopeApplicationModule.RegisterEngines(engine);
        MessageMetadataSchemaApplicationModule.RegisterEngines(engine);
        PayloadSchemaApplicationModule.RegisterEngines(engine);

        DispatchRuleApplicationModule.RegisterEngines(engine);
        RouteDefinitionApplicationModule.RegisterEngines(engine);
        RouteDescriptorApplicationModule.RegisterEngines(engine);
        RouteResolutionApplicationModule.RegisterEngines(engine);

        ContractApplicationModule.RegisterEngines(engine);
        SchemaDefinitionApplicationModule.RegisterEngines(engine);
        SerializationFormatApplicationModule.RegisterEngines(engine);
        VersioningRuleApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows required for the platform classification at D2 —
        // all BCs are single-shot factory inits or direct state transitions.
    }
}
