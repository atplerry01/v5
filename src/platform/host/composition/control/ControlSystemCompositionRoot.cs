using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyDefinition.Application;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyPackage.Application;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyEvaluation.Application;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyEnforcement.Application;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyDecision.Application;
using Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyAudit.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.AccessPolicy.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.Authorization.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.Identity.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.Permission.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.Principal.Application;
using Whycespace.Platform.Host.Composition.Control.AccessControl.Role.Application;
using Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationAssignment.Application;
using Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationDefinition.Application;
using Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationResolution.Application;
using Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationScope.Application;
using Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationState.Application;
using Whycespace.Platform.Host.Composition.Control.Audit.AuditEvent.Application;
using Whycespace.Platform.Host.Composition.Control.Audit.AuditLog.Application;
using Whycespace.Platform.Host.Composition.Control.Audit.AuditQuery.Application;
using Whycespace.Platform.Host.Composition.Control.Audit.AuditRecord.Application;
using Whycespace.Platform.Host.Composition.Control.Audit.AuditTrace.Application;
using Whycespace.Platform.Host.Composition.Control.Observability.SystemAlert.Application;
using Whycespace.Platform.Host.Composition.Control.Observability.SystemHealth.Application;
using Whycespace.Platform.Host.Composition.Control.Observability.SystemMetric.Application;
using Whycespace.Platform.Host.Composition.Control.Observability.SystemSignal.Application;
using Whycespace.Platform.Host.Composition.Control.Observability.SystemTrace.Application;
using Whycespace.Platform.Host.Composition.Control.Scheduling.ExecutionControl.Application;
using Whycespace.Platform.Host.Composition.Control.Scheduling.ScheduleControl.Application;
using Whycespace.Platform.Host.Composition.Control.Scheduling.SystemJob.Application;
using Whycespace.Platform.Host.Composition.Control.SystemReconciliation.ConsistencyCheck.Application;
using Whycespace.Platform.Host.Composition.Control.SystemReconciliation.DiscrepancyDetection.Application;
using Whycespace.Platform.Host.Composition.Control.SystemReconciliation.DiscrepancyResolution.Application;
using Whycespace.Platform.Host.Composition.Control.SystemReconciliation.ReconciliationRun.Application;
using Whycespace.Platform.Host.Composition.Control.SystemReconciliation.SystemVerification.Application;
using Whycespace.Projections.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Projections.Control.SystemPolicy.PolicyPackage;
using Whycespace.Projections.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Projections.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Projections.Control.SystemPolicy.PolicyDecision;
using Whycespace.Projections.Control.SystemPolicy.PolicyAudit;
using Whycespace.Projections.Control.AccessControl.AccessPolicy;
using Whycespace.Projections.Control.AccessControl.Authorization;
using Whycespace.Projections.Control.AccessControl.Identity;
using Whycespace.Projections.Control.AccessControl.Permission;
using Whycespace.Projections.Control.AccessControl.Principal;
using Whycespace.Projections.Control.AccessControl.Role;
using Whycespace.Projections.Control.Configuration.ConfigurationAssignment;
using Whycespace.Projections.Control.Configuration.ConfigurationDefinition;
using Whycespace.Projections.Control.Configuration.ConfigurationResolution;
using Whycespace.Projections.Control.Configuration.ConfigurationScope;
using Whycespace.Projections.Control.Configuration.ConfigurationState;
using Whycespace.Projections.Control.Audit.AuditEvent;
using Whycespace.Projections.Control.Audit.AuditLog;
using Whycespace.Projections.Control.Audit.AuditQuery;
using Whycespace.Projections.Control.Audit.AuditRecord;
using Whycespace.Projections.Control.Audit.AuditTrace;
using Whycespace.Projections.Control.Observability.SystemAlert;
using Whycespace.Projections.Control.Observability.SystemHealth;
using Whycespace.Projections.Control.Observability.SystemMetric;
using Whycespace.Projections.Control.Observability.SystemSignal;
using Whycespace.Projections.Control.Observability.SystemTrace;
using Whycespace.Projections.Control.Scheduling.ExecutionControl;
using Whycespace.Projections.Control.Scheduling.ScheduleControl;
using Whycespace.Projections.Control.Scheduling.SystemJob;
using Whycespace.Projections.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Projections.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Projections.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Projections.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Projections.Control.SystemReconciliation.SystemVerification;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Control;

public sealed class ControlSystemCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── Application modules ──────────────────────────────────────────
        services.AddPolicyDefinitionApplication();
        services.AddPolicyPackageApplication();
        services.AddPolicyEvaluationApplication();
        services.AddPolicyEnforcementApplication();
        services.AddPolicyDecisionApplication();
        services.AddPolicyAuditApplication();

        services.AddAccessPolicyApplication();
        services.AddAuthorizationApplication();
        services.AddIdentityApplication();
        services.AddPermissionApplication();
        services.AddPrincipalApplication();
        services.AddRoleApplication();

        services.AddConfigurationAssignmentApplication();
        services.AddConfigurationDefinitionApplication();
        services.AddConfigurationResolutionApplication();
        services.AddConfigurationScopeApplication();
        services.AddConfigurationStateApplication();

        services.AddAuditEventApplication();
        services.AddAuditLogApplication();
        services.AddAuditQueryApplication();
        services.AddAuditRecordApplication();
        services.AddAuditTraceApplication();

        services.AddSystemAlertApplication();
        services.AddSystemHealthApplication();
        services.AddSystemMetricApplication();
        services.AddSystemSignalApplication();
        services.AddSystemTraceApplication();

        services.AddExecutionControlApplication();
        services.AddScheduleControlApplication();
        services.AddSystemJobApplication();

        services.AddConsistencyCheckApplication();
        services.AddDiscrepancyDetectionApplication();
        services.AddDiscrepancyResolutionApplication();
        services.AddReconciliationRunApplication();
        services.AddSystemVerificationApplication();

        // ── Projection stores ────────────────────────────────────────────
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyDefinitionReadModel>("projection_control_system_policy_policy_definition", "policy_definition_read_model", "PolicyDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyPackageReadModel>("projection_control_system_policy_policy_package", "policy_package_read_model", "PolicyPackage"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyEvaluationReadModel>("projection_control_system_policy_policy_evaluation", "policy_evaluation_read_model", "PolicyEvaluation"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyEnforcementReadModel>("projection_control_system_policy_policy_enforcement", "policy_enforcement_read_model", "PolicyEnforcement"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyDecisionReadModel>("projection_control_system_policy_policy_decision", "policy_decision_read_model", "PolicyDecision"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PolicyAuditReadModel>("projection_control_system_policy_policy_audit", "policy_audit_read_model", "PolicyAudit"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AccessPolicyReadModel>("projection_control_access_control_access_policy", "access_policy_read_model", "AccessPolicy"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuthorizationReadModel>("projection_control_access_control_authorization", "authorization_read_model", "Authorization"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<IdentityReadModel>("projection_control_access_control_identity", "identity_read_model", "Identity"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PermissionReadModel>("projection_control_access_control_permission", "permission_read_model", "Permission"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<PrincipalReadModel>("projection_control_access_control_principal", "principal_read_model", "Principal"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<RoleReadModel>("projection_control_access_control_role", "role_read_model", "Role"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConfigurationAssignmentReadModel>("projection_control_configuration_configuration_assignment", "configuration_assignment_read_model", "ConfigurationAssignment"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConfigurationDefinitionReadModel>("projection_control_configuration_configuration_definition", "configuration_definition_read_model", "ConfigurationDefinition"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConfigurationResolutionReadModel>("projection_control_configuration_configuration_resolution", "configuration_resolution_read_model", "ConfigurationResolution"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConfigurationScopeReadModel>("projection_control_configuration_configuration_scope", "configuration_scope_read_model", "ConfigurationScope"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConfigurationStateReadModel>("projection_control_configuration_configuration_state", "configuration_state_read_model", "ConfigurationState"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuditEventReadModel>("projection_control_audit_audit_event", "audit_event_read_model", "AuditEvent"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuditLogReadModel>("projection_control_audit_audit_log", "audit_log_read_model", "AuditLog"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuditQueryReadModel>("projection_control_audit_audit_query", "audit_query_read_model", "AuditQuery"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuditRecordReadModel>("projection_control_audit_audit_record", "audit_record_read_model", "AuditRecord"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AuditTraceReadModel>("projection_control_audit_audit_trace", "audit_trace_read_model", "AuditTrace"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemAlertReadModel>("projection_control_observability_system_alert", "system_alert_read_model", "SystemAlert"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemHealthReadModel>("projection_control_observability_system_health", "system_health_read_model", "SystemHealth"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemMetricReadModel>("projection_control_observability_system_metric", "system_metric_read_model", "SystemMetric"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemSignalReadModel>("projection_control_observability_system_signal", "system_signal_read_model", "SystemSignal"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemTraceReadModel>("projection_control_observability_system_trace", "system_trace_read_model", "SystemTrace"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ExecutionControlReadModel>("projection_control_scheduling_execution_control", "execution_control_read_model", "ExecutionControl"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ScheduleControlReadModel>("projection_control_scheduling_schedule_control", "schedule_control_read_model", "ScheduleControl"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemJobReadModel>("projection_control_scheduling_system_job", "system_job_read_model", "SystemJob"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ConsistencyCheckReadModel>("projection_control_system_reconciliation_consistency_check", "consistency_check_read_model", "ConsistencyCheck"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DiscrepancyDetectionReadModel>("projection_control_system_reconciliation_discrepancy_detection", "discrepancy_detection_read_model", "DiscrepancyDetection"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<DiscrepancyResolutionReadModel>("projection_control_system_reconciliation_discrepancy_resolution", "discrepancy_resolution_read_model", "DiscrepancyResolution"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<ReconciliationRunReadModel>("projection_control_system_reconciliation_reconciliation_run", "reconciliation_run_read_model", "ReconciliationRun"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<SystemVerificationReadModel>("projection_control_system_reconciliation_system_verification", "system_verification_read_model", "SystemVerification"));

        // ── Projection handlers ──────────────────────────────────────────
        services.AddSingleton(sp => new PolicyDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyDefinitionReadModel>>()));
        services.AddSingleton(sp => new PolicyPackageProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyPackageReadModel>>()));
        services.AddSingleton(sp => new PolicyEvaluationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyEvaluationReadModel>>()));
        services.AddSingleton(sp => new PolicyEnforcementProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyEnforcementReadModel>>()));
        services.AddSingleton(sp => new PolicyDecisionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyDecisionReadModel>>()));
        services.AddSingleton(sp => new PolicyAuditProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PolicyAuditReadModel>>()));

        services.AddSingleton(sp => new AccessPolicyProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AccessPolicyReadModel>>()));
        services.AddSingleton(sp => new AuthorizationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuthorizationReadModel>>()));
        services.AddSingleton(sp => new IdentityProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<IdentityReadModel>>()));
        services.AddSingleton(sp => new PermissionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PermissionReadModel>>()));
        services.AddSingleton(sp => new PrincipalProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PrincipalReadModel>>()));
        services.AddSingleton(sp => new RoleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RoleReadModel>>()));

        services.AddSingleton(sp => new ConfigurationAssignmentProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConfigurationAssignmentReadModel>>()));
        services.AddSingleton(sp => new ConfigurationDefinitionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConfigurationDefinitionReadModel>>()));
        services.AddSingleton(sp => new ConfigurationResolutionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConfigurationResolutionReadModel>>()));
        services.AddSingleton(sp => new ConfigurationScopeProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConfigurationScopeReadModel>>()));
        services.AddSingleton(sp => new ConfigurationStateProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConfigurationStateReadModel>>()));

        services.AddSingleton(sp => new AuditEventProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditEventReadModel>>()));
        services.AddSingleton(sp => new AuditLogProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditLogReadModel>>()));
        services.AddSingleton(sp => new AuditQueryProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditQueryReadModel>>()));
        services.AddSingleton(sp => new AuditRecordProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditRecordReadModel>>()));
        services.AddSingleton(sp => new AuditTraceProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditTraceReadModel>>()));

        services.AddSingleton(sp => new SystemAlertProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemAlertReadModel>>()));
        services.AddSingleton(sp => new SystemHealthProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemHealthReadModel>>()));
        services.AddSingleton(sp => new SystemMetricProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemMetricReadModel>>()));
        services.AddSingleton(sp => new SystemSignalProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemSignalReadModel>>()));
        services.AddSingleton(sp => new SystemTraceProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemTraceReadModel>>()));

        services.AddSingleton(sp => new ExecutionControlProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ExecutionControlReadModel>>()));
        services.AddSingleton(sp => new ScheduleControlProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ScheduleControlReadModel>>()));
        services.AddSingleton(sp => new SystemJobProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemJobReadModel>>()));

        services.AddSingleton(sp => new ConsistencyCheckProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ConsistencyCheckReadModel>>()));
        services.AddSingleton(sp => new DiscrepancyDetectionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DiscrepancyDetectionReadModel>>()));
        services.AddSingleton(sp => new DiscrepancyResolutionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DiscrepancyResolutionReadModel>>()));
        services.AddSingleton(sp => new ReconciliationRunProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ReconciliationRunReadModel>>()));
        services.AddSingleton(sp => new SystemVerificationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SystemVerificationReadModel>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyDefinition(schema);
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyPackage(schema);
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyEvaluation(schema);
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyEnforcement(schema);
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyDecision(schema);
        DomainSchemaCatalog.RegisterControlSystemPolicyPolicyAudit(schema);

        DomainSchemaCatalog.RegisterControlAccessControlAccessPolicy(schema);
        DomainSchemaCatalog.RegisterControlAccessControlAuthorization(schema);
        DomainSchemaCatalog.RegisterControlAccessControlIdentity(schema);
        DomainSchemaCatalog.RegisterControlAccessControlPermission(schema);
        DomainSchemaCatalog.RegisterControlAccessControlPrincipal(schema);
        DomainSchemaCatalog.RegisterControlAccessControlRole(schema);

        DomainSchemaCatalog.RegisterControlConfigurationConfigurationAssignment(schema);
        DomainSchemaCatalog.RegisterControlConfigurationConfigurationDefinition(schema);
        DomainSchemaCatalog.RegisterControlConfigurationConfigurationResolution(schema);
        DomainSchemaCatalog.RegisterControlConfigurationConfigurationScope(schema);
        DomainSchemaCatalog.RegisterControlConfigurationConfigurationState(schema);

        DomainSchemaCatalog.RegisterControlAuditAuditEvent(schema);
        DomainSchemaCatalog.RegisterControlAuditAuditLog(schema);
        DomainSchemaCatalog.RegisterControlAuditAuditQuery(schema);
        DomainSchemaCatalog.RegisterControlAuditAuditRecord(schema);
        DomainSchemaCatalog.RegisterControlAuditAuditTrace(schema);

        DomainSchemaCatalog.RegisterControlObservabilitySystemAlert(schema);
        DomainSchemaCatalog.RegisterControlObservabilitySystemHealth(schema);
        DomainSchemaCatalog.RegisterControlObservabilitySystemMetric(schema);
        DomainSchemaCatalog.RegisterControlObservabilitySystemSignal(schema);
        DomainSchemaCatalog.RegisterControlObservabilitySystemTrace(schema);

        DomainSchemaCatalog.RegisterControlSchedulingExecutionControl(schema);
        DomainSchemaCatalog.RegisterControlSchedulingScheduleControl(schema);
        DomainSchemaCatalog.RegisterControlSchedulingSystemJob(schema);

        DomainSchemaCatalog.RegisterControlSystemReconciliationConsistencyCheck(schema);
        DomainSchemaCatalog.RegisterControlSystemReconciliationDiscrepancyDetection(schema);
        DomainSchemaCatalog.RegisterControlSystemReconciliationDiscrepancyResolution(schema);
        DomainSchemaCatalog.RegisterControlSystemReconciliationReconciliationRun(schema);
        DomainSchemaCatalog.RegisterControlSystemReconciliationSystemVerification(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var policyDefinitionHandler = provider.GetRequiredService<PolicyDefinitionProjectionHandler>();
        projection.Register("PolicyDefinedEvent", policyDefinitionHandler);
        projection.Register("PolicyDeprecatedEvent", policyDefinitionHandler);

        var policyPackageHandler = provider.GetRequiredService<PolicyPackageProjectionHandler>();
        projection.Register("PolicyPackageAssembledEvent", policyPackageHandler);
        projection.Register("PolicyPackageDeployedEvent", policyPackageHandler);
        projection.Register("PolicyPackageRetiredEvent", policyPackageHandler);

        var policyEvaluationHandler = provider.GetRequiredService<PolicyEvaluationProjectionHandler>();
        projection.Register("PolicyEvaluationRecordedEvent", policyEvaluationHandler);
        projection.Register("PolicyEvaluationVerdictIssuedEvent", policyEvaluationHandler);

        var policyEnforcementHandler = provider.GetRequiredService<PolicyEnforcementProjectionHandler>();
        projection.Register("PolicyEnforcedEvent", policyEnforcementHandler);

        var policyDecisionHandler = provider.GetRequiredService<PolicyDecisionProjectionHandler>();
        projection.Register("PolicyDecisionRecordedEvent", policyDecisionHandler);

        var policyAuditHandler = provider.GetRequiredService<PolicyAuditProjectionHandler>();
        projection.Register("PolicyAuditEntryRecordedEvent", policyAuditHandler);
        projection.Register("PolicyAuditEntryReviewedEvent", policyAuditHandler);

        var accessPolicyHandler = provider.GetRequiredService<AccessPolicyProjectionHandler>();
        projection.Register("AccessPolicyDefinedEvent", accessPolicyHandler);
        projection.Register("AccessPolicyActivatedEvent", accessPolicyHandler);
        projection.Register("AccessPolicyRetiredEvent", accessPolicyHandler);

        var authorizationHandler = provider.GetRequiredService<AuthorizationProjectionHandler>();
        projection.Register("AuthorizationGrantedEvent", authorizationHandler);
        projection.Register("AuthorizationRevokedEvent", authorizationHandler);

        var identityHandler = provider.GetRequiredService<IdentityProjectionHandler>();
        projection.Register("IdentityRegisteredEvent", identityHandler);
        projection.Register("IdentitySuspendedEvent", identityHandler);
        projection.Register("IdentityDeactivatedEvent", identityHandler);

        var permissionHandler = provider.GetRequiredService<PermissionProjectionHandler>();
        projection.Register("PermissionDefinedEvent", permissionHandler);
        projection.Register("PermissionDeprecatedEvent", permissionHandler);

        var principalHandler = provider.GetRequiredService<PrincipalProjectionHandler>();
        projection.Register("PrincipalRegisteredEvent", principalHandler);
        projection.Register("PrincipalRoleAssignedEvent", principalHandler);
        projection.Register("PrincipalDeactivatedEvent", principalHandler);

        var roleHandler = provider.GetRequiredService<RoleProjectionHandler>();
        projection.Register("RoleDefinedEvent", roleHandler);
        projection.Register("RolePermissionAddedEvent", roleHandler);
        projection.Register("RoleDeprecatedEvent", roleHandler);

        var configurationAssignmentHandler = provider.GetRequiredService<ConfigurationAssignmentProjectionHandler>();
        projection.Register("ConfigurationAssignedEvent", configurationAssignmentHandler);
        projection.Register("ConfigurationAssignmentRevokedEvent", configurationAssignmentHandler);

        var configurationDefinitionHandler = provider.GetRequiredService<ConfigurationDefinitionProjectionHandler>();
        projection.Register("ConfigurationDefinedEvent", configurationDefinitionHandler);
        projection.Register("ConfigurationDefinitionDeprecatedEvent", configurationDefinitionHandler);

        var configurationResolutionHandler = provider.GetRequiredService<ConfigurationResolutionProjectionHandler>();
        projection.Register("ConfigurationResolvedEvent", configurationResolutionHandler);

        var configurationScopeHandler = provider.GetRequiredService<ConfigurationScopeProjectionHandler>();
        projection.Register("ConfigurationScopeDeclaredEvent", configurationScopeHandler);
        projection.Register("ConfigurationScopeRemovedEvent", configurationScopeHandler);

        var configurationStateHandler = provider.GetRequiredService<ConfigurationStateProjectionHandler>();
        projection.Register("ConfigurationStateSetEvent", configurationStateHandler);
        projection.Register("ConfigurationStateRevokedEvent", configurationStateHandler);

        var auditEventHandler = provider.GetRequiredService<AuditEventProjectionHandler>();
        projection.Register("AuditEventCapturedEvent", auditEventHandler);
        projection.Register("AuditEventSealedEvent", auditEventHandler);

        var auditLogHandler = provider.GetRequiredService<AuditLogProjectionHandler>();
        projection.Register("AuditEntryRecordedEvent", auditLogHandler);

        var auditQueryHandler = provider.GetRequiredService<AuditQueryProjectionHandler>();
        projection.Register("AuditQueryIssuedEvent", auditQueryHandler);
        projection.Register("AuditQueryCompletedEvent", auditQueryHandler);
        projection.Register("AuditQueryExpiredEvent", auditQueryHandler);

        var auditRecordHandler = provider.GetRequiredService<AuditRecordProjectionHandler>();
        projection.Register("AuditRecordRaisedEvent", auditRecordHandler);
        projection.Register("AuditRecordResolvedEvent", auditRecordHandler);

        var auditTraceHandler = provider.GetRequiredService<AuditTraceProjectionHandler>();
        projection.Register("AuditTraceOpenedEvent", auditTraceHandler);
        projection.Register("AuditTraceEventLinkedEvent", auditTraceHandler);
        projection.Register("AuditTraceClosedEvent", auditTraceHandler);

        var systemAlertHandler = provider.GetRequiredService<SystemAlertProjectionHandler>();
        projection.Register("SystemAlertDefinedEvent", systemAlertHandler);
        projection.Register("SystemAlertRetiredEvent", systemAlertHandler);

        var systemHealthHandler = provider.GetRequiredService<SystemHealthProjectionHandler>();
        projection.Register("SystemHealthEvaluatedEvent", systemHealthHandler);
        projection.Register("SystemHealthDegradedEvent", systemHealthHandler);
        projection.Register("SystemHealthRestoredEvent", systemHealthHandler);

        var systemMetricHandler = provider.GetRequiredService<SystemMetricProjectionHandler>();
        projection.Register("SystemMetricDefinedEvent", systemMetricHandler);
        projection.Register("SystemMetricDeprecatedEvent", systemMetricHandler);

        var systemSignalHandler = provider.GetRequiredService<SystemSignalProjectionHandler>();
        projection.Register("SystemSignalDefinedEvent", systemSignalHandler);
        projection.Register("SystemSignalDeprecatedEvent", systemSignalHandler);

        var systemTraceHandler = provider.GetRequiredService<SystemTraceProjectionHandler>();
        projection.Register("SystemTraceSpanStartedEvent", systemTraceHandler);
        projection.Register("SystemTraceSpanCompletedEvent", systemTraceHandler);

        var executionControlHandler = provider.GetRequiredService<ExecutionControlProjectionHandler>();
        projection.Register("ExecutionControlSignalIssuedEvent", executionControlHandler);
        projection.Register("ExecutionControlSignalOutcomeRecordedEvent", executionControlHandler);

        var scheduleControlHandler = provider.GetRequiredService<ScheduleControlProjectionHandler>();
        projection.Register("ScheduleControlDefinedEvent", scheduleControlHandler);
        projection.Register("ScheduleControlSuspendedEvent", scheduleControlHandler);
        projection.Register("ScheduleControlResumedEvent", scheduleControlHandler);
        projection.Register("ScheduleControlRetiredEvent", scheduleControlHandler);

        var systemJobHandler = provider.GetRequiredService<SystemJobProjectionHandler>();
        projection.Register("SystemJobDefinedEvent", systemJobHandler);
        projection.Register("SystemJobDeprecatedEvent", systemJobHandler);

        var consistencyCheckHandler = provider.GetRequiredService<ConsistencyCheckProjectionHandler>();
        projection.Register("ConsistencyCheckInitiatedEvent", consistencyCheckHandler);
        projection.Register("ConsistencyCheckCompletedEvent", consistencyCheckHandler);

        var discrepancyDetectionHandler = provider.GetRequiredService<DiscrepancyDetectionProjectionHandler>();
        projection.Register("DiscrepancyDetectedEvent", discrepancyDetectionHandler);
        projection.Register("DiscrepancyDetectionDismissedEvent", discrepancyDetectionHandler);

        var discrepancyResolutionHandler = provider.GetRequiredService<DiscrepancyResolutionProjectionHandler>();
        projection.Register("DiscrepancyResolutionInitiatedEvent", discrepancyResolutionHandler);
        projection.Register("DiscrepancyResolutionCompletedEvent", discrepancyResolutionHandler);

        var reconciliationRunHandler = provider.GetRequiredService<ReconciliationRunProjectionHandler>();
        projection.Register("ReconciliationRunScheduledEvent", reconciliationRunHandler);
        projection.Register("ReconciliationRunStartedEvent", reconciliationRunHandler);
        projection.Register("ReconciliationRunCompletedEvent", reconciliationRunHandler);
        projection.Register("ReconciliationRunAbortedEvent", reconciliationRunHandler);

        var systemVerificationHandler = provider.GetRequiredService<SystemVerificationProjectionHandler>();
        projection.Register("SystemVerificationInitiatedEvent", systemVerificationHandler);
        projection.Register("SystemVerificationPassedEvent", systemVerificationHandler);
        projection.Register("SystemVerificationFailedEvent", systemVerificationHandler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        PolicyDefinitionApplicationModule.RegisterEngines(engine);
        PolicyPackageApplicationModule.RegisterEngines(engine);
        PolicyEvaluationApplicationModule.RegisterEngines(engine);
        PolicyEnforcementApplicationModule.RegisterEngines(engine);
        PolicyDecisionApplicationModule.RegisterEngines(engine);
        PolicyAuditApplicationModule.RegisterEngines(engine);

        AccessPolicyApplicationModule.RegisterEngines(engine);
        AuthorizationApplicationModule.RegisterEngines(engine);
        IdentityApplicationModule.RegisterEngines(engine);
        PermissionApplicationModule.RegisterEngines(engine);
        PrincipalApplicationModule.RegisterEngines(engine);
        RoleApplicationModule.RegisterEngines(engine);

        ConfigurationAssignmentApplicationModule.RegisterEngines(engine);
        ConfigurationDefinitionApplicationModule.RegisterEngines(engine);
        ConfigurationResolutionApplicationModule.RegisterEngines(engine);
        ConfigurationScopeApplicationModule.RegisterEngines(engine);
        ConfigurationStateApplicationModule.RegisterEngines(engine);

        AuditEventApplicationModule.RegisterEngines(engine);
        AuditLogApplicationModule.RegisterEngines(engine);
        AuditQueryApplicationModule.RegisterEngines(engine);
        AuditRecordApplicationModule.RegisterEngines(engine);
        AuditTraceApplicationModule.RegisterEngines(engine);

        SystemAlertApplicationModule.RegisterEngines(engine);
        SystemHealthApplicationModule.RegisterEngines(engine);
        SystemMetricApplicationModule.RegisterEngines(engine);
        SystemSignalApplicationModule.RegisterEngines(engine);
        SystemTraceApplicationModule.RegisterEngines(engine);

        ExecutionControlApplicationModule.RegisterEngines(engine);
        ScheduleControlApplicationModule.RegisterEngines(engine);
        SystemJobApplicationModule.RegisterEngines(engine);

        ConsistencyCheckApplicationModule.RegisterEngines(engine);
        DiscrepancyDetectionApplicationModule.RegisterEngines(engine);
        DiscrepancyResolutionApplicationModule.RegisterEngines(engine);
        ReconciliationRunApplicationModule.RegisterEngines(engine);
        SystemVerificationApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
    }
}
