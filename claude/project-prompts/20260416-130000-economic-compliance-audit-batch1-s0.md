# Batch 1 (S0) — Economic-System Remediation: Compliance/Audit Domain End-to-End

## TITLE
Economic-Compliance-Audit Batch 1 (S0): Operational end-to-end

## CONTEXT
Executing Batch 1 of the economic-system remediation plan for the compliance/audit domain. Prior scaffolding passes produced the domain aggregate, events, value objects, application handlers, read model, projection DDL, projection handler/reducer, event schemas, Kafka topic declarations, controller, and policy bindings. The remaining gap is an E2E test that exercises event → projection → query against the real runtime (real Kafka, real Postgres, real API host).

## OBJECTIVE
Prove that the audit compliance pipeline is fully operational end-to-end:

1. Kafka topic `whyce.economic.compliance.audit.events` provisioned via `infrastructure/event-fabric/kafka/create-topics.sh`.
2. Projection DDL `projection_economic_compliance_audit.audit_record_read_model` present under `infrastructure/data/postgres/projections/economic/compliance/audit/001_projection.sql`.
3. Read policy bindings registered via `CompliancePolicyModule.AddCompliancePolicyBindings()` for create/finalize/read commands.
4. E2E test under `tests/e2e/economic/compliance/audit/` that posts `CreateAuditRecordCommand` via HTTP, waits for the projection to materialise, posts `FinalizeAuditRecordCommand`, and asserts the read-model query returns the full trail.

## CONSTRAINTS
- No simulation. No stubs. Tests must speak to the real API host, real Postgres projection schema, real Kafka topic.
- Do not modify guard/audit files.
- Preserve deterministic id derivation on both server and test sides (`economic:compliance:audit:{SourceDomain}:{SourceAggregateId}:{SourceEventId}`).
- Do not introduce new HTTP endpoints; the AuditController at `src/platform/api/controllers/economic/compliance/audit/AuditController.cs` already exposes create/finalize/get.
- Follow the per-domain E2E convention established by `tests/e2e/economic/capital/_setup/` and `tests/e2e/economic/exchange/_setup/` (dedicated `_setup/` directory per domain; no refactor of shared helpers).

## EXECUTION STEPS
1. Verify existing components: Kafka topic declaration, projection DDL, policy binding, controller, read model. (Confirmed existing.)
2. Create `tests/e2e/economic/compliance/_setup/` with `ComplianceE2EConfig`, `ComplianceE2EFixture`, `ComplianceE2ECollection`, `ComplianceApiEnvelope`, `ComplianceProjectionVerifier` mirroring the Capital/Exchange pattern.
3. Create `tests/e2e/economic/compliance/audit/AuditRecordE2ETests.cs` with three tests:
   - Happy path — create → projection contains `Status=Draft` with full evidence → GET returns read model.
   - Lifecycle — create → finalize → projection contains `Status=Finalized` with `FinalizedAt` populated → GET reflects final state.
   - Failure — finalize of unknown aggregate → HTTP 400 → no projection row written.
4. Build the `Whycespace.Tests.E2E` project to confirm compilation against the real contracts.
5. Run the audit sweep per $1b and capture any drift.

## OUTPUT FORMAT
Per task: implementation summary, files created/modified, verification evidence, remaining risks.

## VALIDATION CRITERIA
- Topic `whyce.economic.compliance.audit.events` exists in `create-topics.sh`.
- Projection DDL file exists with `audit_record_read_model` table and indexes.
- `AddCompliancePolicyBindings` registers all three `CommandPolicyBinding` entries.
- `tests/e2e/economic/compliance/audit/AuditRecordE2ETests.cs` compiles and encodes event → projection → query flow using `ApiEnvelope`-style helpers and `PollUntilPresentAsync` / `PollUntilStatusAsync`.
- All components wired through the existing composition root (no stubs, no mocks).
