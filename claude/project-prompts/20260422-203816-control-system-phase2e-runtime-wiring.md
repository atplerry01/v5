# Phase 2E — Control-System Runtime Wiring: Complete Vertical Slice

## TITLE
Control-System Phase 2E: Runtime wiring for all 35 bounded contexts across 7 contexts

## CONTEXT
Phase 2A–2D delivered the domain, engine, projections, contracts, and API layers for all 35 control-system BCs across 7 contexts:
- system-policy (6 BCs): PolicyDefinition, PolicyPackage, PolicyEvaluation, PolicyEnforcement, PolicyDecision, PolicyAudit
- access-control (6 BCs): AccessPolicy, Authorization, Identity, Permission, Principal, Role
- configuration (5 BCs): ConfigurationAssignment, ConfigurationDefinition, ConfigurationResolution, ConfigurationScope, ConfigurationState
- audit (5 BCs): AuditEvent, AuditLog, AuditQuery, AuditRecord, AuditTrace
- observability (5 BCs): SystemAlert, SystemHealth, SystemMetric, SystemSignal, SystemTrace
- scheduling (3 BCs): ExecutionControl, ScheduleControl, SystemJob
- system-reconciliation (5 BCs): ConsistencyCheck, DiscrepancyDetection, DiscrepancyResolution, ReconciliationRun, SystemVerification

Phase 2E wires the runtime layer: schema modules (event-fabric), application modules (host composition), the composition root, DomainSchemaCatalog updates, and BootstrapModuleCatalog registration.

## CLASSIFICATION
classification: control-system  
context: all 7 contexts (system-policy, access-control, configuration, audit, observability, scheduling, system-reconciliation)  
domain: all 35 BCs  
phase: 2E (Runtime Wiring)  
policy: WHYCEPOLICY-CONTROL-SYSTEM-RUNTIME-WIRING-01

## OBJECTIVE
Deliver complete runtime wiring for all 35 control-system BCs:
1. 35 schema modules in `src/runtime/event-fabric/domain-schemas/`
2. 35 application modules in `src/platform/host/composition/control/{context}/{domain}/application/`
3. 1 `ControlSystemCompositionRoot.cs` implementing `IDomainBootstrapModule` with all 5 methods
4. `DomainSchemaCatalog.cs` updated with 35 new `RegisterControl*` static methods
5. `BootstrapModuleCatalog.cs` updated to include `new ControlSystemCompositionRoot()`

## CONSTRAINTS
- Anti-Drift ($5): no architecture changes, no new patterns
- Layer purity ($7): domain = zero dependencies; schema modules reference domain events via alias only
- Determinism ($9): no Guid.NewGuid, no DateTime.UtcNow, use IClock/IIdGenerator
- Schema module pattern: `ISchemaModule.Register(ISchemaSink)` → `sink.RegisterSchema()` + `sink.RegisterPayloadMapper()`
- Application module pattern: `static class {Domain}ApplicationModule` + `Add{Domain}Application(IServiceCollection)` + `static void RegisterEngines(IEngineRegistry)`
- Composition root implements exactly: `RegisterServices`, `RegisterSchema`, `RegisterProjections`, `RegisterEngines`, `RegisterWorkflows`
- G-COMPLOAD-01: BootstrapModuleCatalog.All is the single seam; Program.cs references no domain module by name
- ID bridge: `Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N")` for 64-char hex VO → Guid
- Enum serialization: `.ToString()` in mapper; `Enum.Parse<T>(value, true)` in handler
- ProjectionStoreFactory.Create<T> takes 3 args: schema name, table name, entity name

## EXECUTION STEPS
1. Create 35 schema modules (one per BC), each named `Control{Context}{Domain}SchemaModule.cs`
2. Create 35 application modules, each in `{context}/{domain}/application/` subfolder
3. Update `DomainSchemaCatalog.cs` with 35 new `RegisterControl*` methods
4. Create `ControlSystemCompositionRoot.cs` as full `IDomainBootstrapModule` implementation
5. Update `BootstrapModuleCatalog.cs` to include `new ControlSystemCompositionRoot()`
6. Run post-execution audit sweep per $1b

## OUTPUT FORMAT
All files created/modified in-place. No documentation artifacts. No intermediate prompt files.

## VALIDATION CRITERIA
- 35 schema module files (Control*.cs) in `src/runtime/event-fabric/domain-schemas/`
- 35 application module files (*ApplicationModule.cs) in `src/platform/host/composition/control/`
- 35 `RegisterControl*` methods in `DomainSchemaCatalog.cs`
- 35 `DomainSchemaCatalog.RegisterControl*()` calls in `ControlSystemCompositionRoot.RegisterSchema`
- 35 `ApplicationModule.RegisterEngines()` calls in `ControlSystemCompositionRoot.RegisterEngines`
- 35 `Add*Application()` calls in `ControlSystemCompositionRoot.RegisterServices`
- 79 `projection.Register()` calls in `ControlSystemCompositionRoot.RegisterProjections`
- Zero determinism violations (Guid.NewGuid/DateTime.UtcNow) in new files
- Zero domain layer imports in application modules
- Zero stubs (NotImplementedException/TODO) in new files
- `BootstrapModuleCatalog.All` includes `new ControlSystemCompositionRoot()`
- `Program.cs` references no domain module by name (G-COMPLOAD-01 maintained)

## AUDIT RESULTS (Phase 2E Post-Execution Sweep)
STATUS: PASS (with pre-existing build errors captured in new-rules)

| Check | Result |
|-------|--------|
| Schema module count | ✅ 35 |
| Application module count | ✅ 35 |
| DomainSchemaCatalog RegisterControl* count | ✅ 35 |
| Composition root IDomainBootstrapModule | ✅ implemented |
| Composition root 5-method coverage | ✅ all 5 methods |
| DomainSchemaCatalog calls in RegisterSchema | ✅ 35 |
| ApplicationModule.RegisterEngines calls | ✅ 35 |
| Add*Application service calls | ✅ 35 |
| projection.Register calls | ✅ 79 |
| Determinism violations (new files) | ✅ 0 |
| Domain imports in application modules | ✅ 0 |
| Stubs in new files | ✅ 0 |
| BootstrapModuleCatalog includes ControlSystemCompositionRoot | ✅ confirmed |
| Program.cs direct domain module refs | ✅ 0 (G-COMPLOAD-01 held) |
| Control-system E1XD aggregate conformance | ✅ all pass |
| Pre-existing build errors (platform-system) | ⚠️ 2 (captured in new-rules 20260422-203754-domain.md, unrelated to Phase 2E) |
