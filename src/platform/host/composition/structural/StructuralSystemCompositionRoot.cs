using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Invariant.EconomicBinding;
using Whycespace.Platform.Host.Composition.Structural.Adapters;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Administration.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Authority.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Cluster.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Lifecycle.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Provider.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Spv.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Subcluster.Application;
using Whycespace.Platform.Host.Composition.Structural.Cluster.Topology.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Assignment.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Eligibility.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Governance.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Incentive.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Operator.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Participant.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Performance.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Reputation.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Sanction.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Sponsorship.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Stewardship.Application;
using Whycespace.Platform.Host.Composition.Structural.Humancapital.Workforce.Application;
using Whycespace.Platform.Host.Composition.Structural.Structure.Classification.Application;
using Whycespace.Platform.Host.Composition.Structural.Structure.HierarchyDefinition.Application;
using Whycespace.Platform.Host.Composition.Structural.Structure.TopologyDefinition.Application;
using Whycespace.Platform.Host.Composition.Structural.Structure.TypeDefinition.Application;
using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Administration;
using Whycespace.Projections.Structural.Cluster.Authority;
using Whycespace.Projections.Structural.Cluster.Cluster;
using Whycespace.Projections.Structural.Cluster.Lifecycle;
using Whycespace.Projections.Structural.Cluster.Provider;
using Whycespace.Projections.Structural.Cluster.Spv;
using Whycespace.Projections.Structural.Cluster.Subcluster;
using Whycespace.Projections.Structural.Cluster.Topology;
using Whycespace.Projections.Structural.Humancapital.Assignment;
using Whycespace.Projections.Structural.Humancapital.Eligibility;
using Whycespace.Projections.Structural.Humancapital.Governance;
using Whycespace.Projections.Structural.Humancapital.Incentive;
using Whycespace.Projections.Structural.Humancapital.Operator;
using Whycespace.Projections.Structural.Humancapital.Participant;
using Whycespace.Projections.Structural.Humancapital.Performance;
using Whycespace.Projections.Structural.Humancapital.Reputation;
using Whycespace.Projections.Structural.Humancapital.Sanction;
using Whycespace.Projections.Structural.Humancapital.Sponsorship;
using Whycespace.Projections.Structural.Humancapital.Stewardship;
using Whycespace.Projections.Structural.Humancapital.Workforce;
using Whycespace.Projections.Structural.Structure.Classification;
using Whycespace.Projections.Structural.Structure.HierarchyDefinition;
using Whycespace.Projections.Structural.Structure.TopologyDefinition;
using Whycespace.Projections.Structural.Structure.TypeDefinition;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;
using Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;
using Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;
using Whycespace.Shared.Contracts.Structural.Humancapital.Governance;
using Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;
using Whycespace.Shared.Contracts.Structural.Humancapital.Operator;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;
using Whycespace.Shared.Contracts.Structural.Humancapital.Performance;
using Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;
using Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;
using Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Platform.Host.Composition.Structural;

/// <summary>
/// Composition root for the structural classification.
///
/// E1→EX delivery:
///   - phase 1: structure context (4 BCs, 14 events) — DELIVERED.
///   - phase 2: cluster context (8 BCs, 50 events) — DELIVERED.
///   - phase 3a: humancapital — participant + assignment wired against
///     IStructuralParentLookup adapter.
///   - phase 3b: cluster *WithParent command variants — administration,
///     authority, provider, spv, subcluster.
///   - phase 3c: humancapital D0 promotion + wiring for eligibility,
///     governance, incentive, operator, performance, reputation,
///     sanction, sponsorship, stewardship, workforce — ALL DELIVERED.
/// </summary>
public sealed class StructuralSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── Structure context ────────────────────────────────────

        services.AddTypeDefinitionApplication();
        services.AddClassificationApplication();
        services.AddHierarchyDefinitionApplication();
        services.AddTopologyDefinitionApplication();

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TypeDefinitionReadModel>(
                    "projection_structural_structure_type_definition",
                    "type_definition_read_model",
                    "TypeDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ClassificationReadModel>(
                    "projection_structural_structure_classification",
                    "classification_read_model",
                    "Classification"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<HierarchyDefinitionReadModel>(
                    "projection_structural_structure_hierarchy_definition",
                    "hierarchy_definition_read_model",
                    "HierarchyDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TopologyDefinitionReadModel>(
                    "projection_structural_structure_topology_definition",
                    "topology_definition_read_model",
                    "TopologyDefinition"));

        services.AddSingleton(sp => new TypeDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TypeDefinitionReadModel>>()));
        services.AddSingleton(sp => new ClassificationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ClassificationReadModel>>()));
        services.AddSingleton(sp => new HierarchyDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<HierarchyDefinitionReadModel>>()));
        services.AddSingleton(sp => new TopologyDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TopologyDefinitionReadModel>>()));

        // ── Cluster context (8 BCs, 50 events) ─────────────────────

        services.AddAdministrationApplication();
        services.AddAuthorityApplication();
        services.AddClusterApplication();
        services.AddLifecycleApplication();
        services.AddProviderApplication();
        services.AddSpvApplication();
        services.AddSubclusterApplication();
        services.AddTopologyApplication();

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AdministrationReadModel>(
                    "projection_structural_cluster_administration",
                    "administration_read_model",
                    "Administration"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuthorityReadModel>(
                    "projection_structural_cluster_authority",
                    "authority_read_model",
                    "Authority"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ClusterReadModel>(
                    "projection_structural_cluster_cluster",
                    "cluster_read_model",
                    "Cluster"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<LifecycleReadModel>(
                    "projection_structural_cluster_lifecycle",
                    "lifecycle_read_model",
                    "Lifecycle"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ProviderReadModel>(
                    "projection_structural_cluster_provider",
                    "provider_read_model",
                    "Provider"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SpvReadModel>(
                    "projection_structural_cluster_spv",
                    "spv_read_model",
                    "Spv"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SubclusterReadModel>(
                    "projection_structural_cluster_subcluster",
                    "subcluster_read_model",
                    "Subcluster"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TopologyReadModel>(
                    "projection_structural_cluster_topology",
                    "topology_read_model",
                    "Topology"));

        services.AddSingleton(sp => new AdministrationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AdministrationReadModel>>()));
        services.AddSingleton(sp => new AuthorityProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuthorityReadModel>>()));
        services.AddSingleton(sp => new ClusterProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ClusterReadModel>>()));
        services.AddSingleton(sp => new LifecycleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LifecycleReadModel>>()));
        services.AddSingleton(sp => new ProviderProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ProviderReadModel>>()));
        services.AddSingleton(sp => new SpvProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SpvReadModel>>()));
        services.AddSingleton(sp => new SubclusterProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SubclusterReadModel>>()));
        services.AddSingleton(sp => new TopologyProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TopologyReadModel>>()));

        // ── Humancapital context (12 BCs) ─────────────────────────

        services.AddParticipantApplication();
        services.AddAssignmentApplication();
        services.AddEligibilityApplication();
        services.AddGovernanceApplication();
        services.AddIncentiveApplication();
        services.AddOperatorApplication();
        services.AddPerformanceApplication();
        services.AddReputationApplication();
        services.AddSanctionApplication();
        services.AddSponsorshipApplication();
        services.AddStewardshipApplication();
        services.AddWorkforceApplication();

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ParticipantReadModel>(
                    "projection_structural_humancapital_participant",
                    "participant_read_model",
                    "Participant"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AssignmentReadModel>(
                    "projection_structural_humancapital_assignment",
                    "assignment_read_model",
                    "Assignment"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<EligibilityReadModel>(
                    "projection_structural_humancapital_eligibility",
                    "eligibility_read_model",
                    "Eligibility"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<GovernanceReadModel>(
                    "projection_structural_humancapital_governance",
                    "governance_read_model",
                    "Governance"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<IncentiveReadModel>(
                    "projection_structural_humancapital_incentive",
                    "incentive_read_model",
                    "Incentive"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<OperatorReadModel>(
                    "projection_structural_humancapital_operator",
                    "operator_read_model",
                    "Operator"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PerformanceReadModel>(
                    "projection_structural_humancapital_performance",
                    "performance_read_model",
                    "Performance"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ReputationReadModel>(
                    "projection_structural_humancapital_reputation",
                    "reputation_read_model",
                    "Reputation"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SanctionReadModel>(
                    "projection_structural_humancapital_sanction",
                    "sanction_read_model",
                    "Sanction"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SponsorshipReadModel>(
                    "projection_structural_humancapital_sponsorship",
                    "sponsorship_read_model",
                    "Sponsorship"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<StewardshipReadModel>(
                    "projection_structural_humancapital_stewardship",
                    "stewardship_read_model",
                    "Stewardship"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<WorkforceReadModel>(
                    "projection_structural_humancapital_workforce",
                    "workforce_read_model",
                    "Workforce"));

        services.AddSingleton(sp => new ParticipantProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ParticipantReadModel>>()));
        services.AddSingleton(sp => new AssignmentProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AssignmentReadModel>>()));
        services.AddSingleton(sp => new EligibilityProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EligibilityReadModel>>()));
        services.AddSingleton(sp => new GovernanceProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<GovernanceReadModel>>()));
        services.AddSingleton(sp => new IncentiveProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<IncentiveReadModel>>()));
        services.AddSingleton(sp => new OperatorProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<OperatorReadModel>>()));
        services.AddSingleton(sp => new PerformanceProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PerformanceReadModel>>()));
        services.AddSingleton(sp => new ReputationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ReputationReadModel>>()));
        services.AddSingleton(sp => new SanctionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SanctionReadModel>>()));
        services.AddSingleton(sp => new SponsorshipProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SponsorshipReadModel>>()));
        services.AddSingleton(sp => new StewardshipProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<StewardshipReadModel>>()));
        services.AddSingleton(sp => new WorkforceProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<WorkforceReadModel>>()));

        // ── Cross-BC state lookup: IStructuralParentLookup ──
        // Projection-backed adapter over cluster + authority read models.
        // Consumed by Subcluster.Define(parentLookup), ParticipantAggregate.Place,
        // AssignmentAggregate.Assign.
        services.AddSingleton<IStructuralParentLookup, StructuralParentLookupAdapter>();

        // Cross-system invariants (domain policies) — pure singletons.
        // Per claude/templates/delivery-pattern/03-runtime-wiring.md § 6b.
        services.AddSingleton<EconomicEntityMustHaveStructuralOwnerPolicy>();
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        // structure context
        DomainSchemaCatalog.RegisterStructuralStructureTypeDefinition(schema);
        DomainSchemaCatalog.RegisterStructuralStructureClassification(schema);
        DomainSchemaCatalog.RegisterStructuralStructureHierarchyDefinition(schema);
        DomainSchemaCatalog.RegisterStructuralStructureTopologyDefinition(schema);

        // cluster context
        DomainSchemaCatalog.RegisterStructuralClusterAdministration(schema);
        DomainSchemaCatalog.RegisterStructuralClusterAuthority(schema);
        DomainSchemaCatalog.RegisterStructuralClusterCluster(schema);
        DomainSchemaCatalog.RegisterStructuralClusterLifecycle(schema);
        DomainSchemaCatalog.RegisterStructuralClusterProvider(schema);
        DomainSchemaCatalog.RegisterStructuralClusterSpv(schema);
        DomainSchemaCatalog.RegisterStructuralClusterSubcluster(schema);
        DomainSchemaCatalog.RegisterStructuralClusterTopology(schema);

        // humancapital context (12 BCs)
        DomainSchemaCatalog.RegisterStructuralHumancapitalParticipant(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalAssignment(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalEligibility(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalGovernance(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalIncentive(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalOperator(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalPerformance(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalReputation(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalSanction(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalSponsorship(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalStewardship(schema);
        DomainSchemaCatalog.RegisterStructuralHumancapitalWorkforce(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var typeDefinitionHandler = provider.GetRequiredService<TypeDefinitionProjectionHandler>();
        projection.Register("TypeDefinitionDefinedEvent", typeDefinitionHandler);
        projection.Register("TypeDefinitionActivatedEvent", typeDefinitionHandler);
        projection.Register("TypeDefinitionRetiredEvent", typeDefinitionHandler);

        var classificationHandler = provider.GetRequiredService<ClassificationProjectionHandler>();
        projection.Register("ClassificationDefinedEvent", classificationHandler);
        projection.Register("ClassificationActivatedEvent", classificationHandler);
        projection.Register("ClassificationDeprecatedEvent", classificationHandler);

        var hierarchyDefinitionHandler = provider.GetRequiredService<HierarchyDefinitionProjectionHandler>();
        projection.Register("HierarchyDefinitionDefinedEvent", hierarchyDefinitionHandler);
        projection.Register("HierarchyDefinitionValidatedEvent", hierarchyDefinitionHandler);
        projection.Register("HierarchyDefinitionLockedEvent", hierarchyDefinitionHandler);

        var topologyDefinitionHandler = provider.GetRequiredService<TopologyDefinitionProjectionHandler>();
        projection.Register("TopologyDefinitionCreatedEvent", topologyDefinitionHandler);
        projection.Register("TopologyDefinitionActivatedEvent", topologyDefinitionHandler);
        projection.Register("TopologyDefinitionSuspendedEvent", topologyDefinitionHandler);
        projection.Register("TopologyDefinitionReactivatedEvent", topologyDefinitionHandler);
        projection.Register("TopologyDefinitionRetiredEvent", topologyDefinitionHandler);

        // ── cluster context ────────────────────────────────────────

        var administrationHandler = provider.GetRequiredService<AdministrationProjectionHandler>();
        projection.Register("AdministrationEstablishedEvent", administrationHandler);
        projection.Register("AdministrationAttachedEvent", administrationHandler);
        projection.Register("AdministrationBindingValidatedEvent", administrationHandler);
        projection.Register("AdministrationActivatedEvent", administrationHandler);
        projection.Register("AdministrationSuspendedEvent", administrationHandler);
        projection.Register("AdministrationRetiredEvent", administrationHandler);

        var authorityHandler = provider.GetRequiredService<AuthorityProjectionHandler>();
        projection.Register("AuthorityEstablishedEvent", authorityHandler);
        projection.Register("AuthorityAttachedEvent", authorityHandler);
        projection.Register("AuthorityBindingValidatedEvent", authorityHandler);
        projection.Register("AuthorityActivatedEvent", authorityHandler);
        projection.Register("AuthorityRevokedEvent", authorityHandler);
        projection.Register("AuthoritySuspendedEvent", authorityHandler);
        projection.Register("AuthorityReactivatedEvent", authorityHandler);
        projection.Register("AuthorityRetiredEvent", authorityHandler);

        var clusterHandler = provider.GetRequiredService<ClusterProjectionHandler>();
        projection.Register("ClusterDefinedEvent", clusterHandler);
        projection.Register("ClusterActivatedEvent", clusterHandler);
        projection.Register("ClusterArchivedEvent", clusterHandler);
        projection.Register("ClusterAuthorityBoundEvent", clusterHandler);
        projection.Register("ClusterAuthorityReleasedEvent", clusterHandler);
        projection.Register("ClusterAdministrationBoundEvent", clusterHandler);
        projection.Register("ClusterAdministrationReleasedEvent", clusterHandler);

        var lifecycleHandler = provider.GetRequiredService<LifecycleProjectionHandler>();
        projection.Register("LifecycleDefinedEvent", lifecycleHandler);
        projection.Register("LifecycleTransitionedEvent", lifecycleHandler);
        projection.Register("LifecycleCompletedEvent", lifecycleHandler);

        var providerHandler = provider.GetRequiredService<ProviderProjectionHandler>();
        projection.Register("ProviderRegisteredEvent", providerHandler);
        projection.Register("ProviderAttachedEvent", providerHandler);
        projection.Register("ProviderBindingValidatedEvent", providerHandler);
        projection.Register("ProviderActivatedEvent", providerHandler);
        projection.Register("ProviderSuspendedEvent", providerHandler);
        projection.Register("ProviderReactivatedEvent", providerHandler);
        projection.Register("ProviderRetiredEvent", providerHandler);

        var spvHandler = provider.GetRequiredService<SpvProjectionHandler>();
        projection.Register("SpvCreatedEvent", spvHandler);
        projection.Register("SpvAttachedEvent", spvHandler);
        projection.Register("SpvBindingValidatedEvent", spvHandler);
        projection.Register("SpvActivatedEvent", spvHandler);
        projection.Register("SpvSuspendedEvent", spvHandler);
        projection.Register("SpvClosedEvent", spvHandler);
        projection.Register("SpvReactivatedEvent", spvHandler);
        projection.Register("SpvRetiredEvent", spvHandler);

        var subclusterHandler = provider.GetRequiredService<SubclusterProjectionHandler>();
        projection.Register("SubclusterDefinedEvent", subclusterHandler);
        projection.Register("SubclusterAttachedEvent", subclusterHandler);
        projection.Register("SubclusterBindingValidatedEvent", subclusterHandler);
        projection.Register("SubclusterActivatedEvent", subclusterHandler);
        projection.Register("SubclusterSuspendedEvent", subclusterHandler);
        projection.Register("SubclusterReactivatedEvent", subclusterHandler);
        projection.Register("SubclusterArchivedEvent", subclusterHandler);
        projection.Register("SubclusterRetiredEvent", subclusterHandler);

        var topologyHandler = provider.GetRequiredService<TopologyProjectionHandler>();
        projection.Register("TopologyDefinedEvent", topologyHandler);
        projection.Register("TopologyValidatedEvent", topologyHandler);
        projection.Register("TopologyLockedEvent", topologyHandler);

        // ── humancapital context (partial) ──────────────────────────

        var participantHandler = provider.GetRequiredService<ParticipantProjectionHandler>();
        projection.Register("ParticipantRegisteredEvent", participantHandler);
        projection.Register("ParticipantPlacedEvent", participantHandler);

        var assignmentHandler = provider.GetRequiredService<AssignmentProjectionHandler>();
        projection.Register("AssignmentAssignedEvent", assignmentHandler);

        var eligibilityHandler = provider.GetRequiredService<EligibilityProjectionHandler>();
        projection.Register("EligibilityCreatedEvent", eligibilityHandler);

        var governanceHandler = provider.GetRequiredService<GovernanceProjectionHandler>();
        projection.Register("GovernanceCreatedEvent", governanceHandler);

        var incentiveHandler = provider.GetRequiredService<IncentiveProjectionHandler>();
        projection.Register("IncentiveCreatedEvent", incentiveHandler);

        var operatorHandler = provider.GetRequiredService<OperatorProjectionHandler>();
        projection.Register("OperatorCreatedEvent", operatorHandler);

        var performanceHandler = provider.GetRequiredService<PerformanceProjectionHandler>();
        projection.Register("PerformanceCreatedEvent", performanceHandler);

        var reputationHandler = provider.GetRequiredService<ReputationProjectionHandler>();
        projection.Register("ReputationCreatedEvent", reputationHandler);

        var sanctionHandler = provider.GetRequiredService<SanctionProjectionHandler>();
        projection.Register("SanctionCreatedEvent", sanctionHandler);

        var sponsorshipHandler = provider.GetRequiredService<SponsorshipProjectionHandler>();
        projection.Register("SponsorshipCreatedEvent", sponsorshipHandler);

        var stewardshipHandler = provider.GetRequiredService<StewardshipProjectionHandler>();
        projection.Register("StewardshipCreatedEvent", stewardshipHandler);

        var workforceHandler = provider.GetRequiredService<WorkforceProjectionHandler>();
        projection.Register("WorkforceCreatedEvent", workforceHandler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        // structure context
        TypeDefinitionApplicationModule.RegisterEngines(engine);
        ClassificationApplicationModule.RegisterEngines(engine);
        HierarchyDefinitionApplicationModule.RegisterEngines(engine);
        TopologyDefinitionApplicationModule.RegisterEngines(engine);

        // cluster context
        AdministrationApplicationModule.RegisterEngines(engine);
        AuthorityApplicationModule.RegisterEngines(engine);
        ClusterApplicationModule.RegisterEngines(engine);
        LifecycleApplicationModule.RegisterEngines(engine);
        ProviderApplicationModule.RegisterEngines(engine);
        SpvApplicationModule.RegisterEngines(engine);
        SubclusterApplicationModule.RegisterEngines(engine);
        TopologyApplicationModule.RegisterEngines(engine);

        // humancapital context (12 BCs)
        ParticipantApplicationModule.RegisterEngines(engine);
        AssignmentApplicationModule.RegisterEngines(engine);
        EligibilityApplicationModule.RegisterEngines(engine);
        GovernanceApplicationModule.RegisterEngines(engine);
        IncentiveApplicationModule.RegisterEngines(engine);
        OperatorApplicationModule.RegisterEngines(engine);
        PerformanceApplicationModule.RegisterEngines(engine);
        ReputationApplicationModule.RegisterEngines(engine);
        SanctionApplicationModule.RegisterEngines(engine);
        SponsorshipApplicationModule.RegisterEngines(engine);
        StewardshipApplicationModule.RegisterEngines(engine);
        WorkforceApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows required for structure context at phase 1 —
        // all BCs are single-shot transitions / factory-inits only.
    }
}
