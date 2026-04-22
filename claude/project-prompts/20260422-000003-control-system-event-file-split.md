# Domain Event File Split — All Systems

## TITLE
S1 Correction: one-file-one-event enforcement across all domain systems

## CONTEXT
Classification: control-system, trust-system, structural-system, operational-system, orchestration-system
Violation: S1 — multiple domain event classes co-located in a single file
Authorization: explicitly granted to split, rename, move event files

## OBJECTIVE
Enforce one-file = one-event = one-fact = one-contract across all domain event folders.

## CONSTRAINTS
- Do NOT change event payload
- Do NOT change event semantics
- Do NOT introduce new abstractions
- Do NOT modify domain logic

## EXECUTION STEPS
1. Detect all files under src/domain/**/event/*.cs containing > 1 event class
2. For each violation: create one file per event, named after event class exactly
3. Replace original multi-event file with empty namespace stub (avoids duplicate symbols, remains valid C#)
4. Validate naming: file name == class name, past tense, single fact

## VIOLATIONS DETECTED (35 files)

### control-system (12)
- access-control/authorization/event/AuthorizationEvents.cs (2)
- access-control/permission/event/PermissionEvents.cs (2)
- access-control/role/event/RoleDefinedEvent.cs (3)
- configuration/configuration-definition/event/ConfigurationDefinitionEvents.cs (2)
- configuration/configuration-scope/event/ConfigurationScopeEvents.cs (2)
- configuration/configuration-state/event/ConfigurationStateEvents.cs (2)
- observability/system-alert/event/SystemAlertEvents.cs (2)
- observability/system-metric/event/SystemMetricEvents.cs (2)
- observability/system-trace/event/SystemTraceEvents.cs (2)
- orchestration/execution-control/event/ExecutionControlEvents.cs (2)
- orchestration/schedule-control/event/ScheduleControlEvents.cs (4)
- orchestration/system-job/event/SystemJobEvents.cs (2)

### trust-system (13)
- identity/identity-graph, access/authorization, access/role, access/session,
  identity/consent, identity/credential, identity/federation, identity/service-identity,
  identity/verification, access/grant, access/request, identity/device, identity/trust

### structural-system (9)
- cluster/lifecycle, cluster/topology, cluster/cluster, cluster/authority, cluster/spv,
  cluster/subcluster, structure/hierarchy-definition, structure/type-definition,
  structure/topology-definition

### operational-system (1)
- incident-response/incident/event/IncidentCreatedEvent.cs (4)

### orchestration-system (1)
- workflow/instance/event/InstanceCreatedEvent.cs (5)

## VALIDATION CRITERIA
- every event class is in its own file
- file name exactly matches class name
- original multi-event files replaced with empty stubs
- no duplicate class declarations
