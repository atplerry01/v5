# PROMPT — RUNTIME DOMAIN ISOLATION (RESOLVER-BASED)

## CLASSIFICATION
runtime / enforcement / isolation

## CONTEXT
Submitted 2026-04-07. Domain leakage (Todo references) exists in `src/runtime/`, `src/platform/host/`, and `src/platform/host/adapters/`. The user proposed a resolver/registry pattern that preserves the existing `RuntimeControlPlane` + middleware pipeline (no new dispatch path).

This prompt is stored per CLAUDE.md $2. It supersedes the prior `DomainRegistry` variant which proposed a parallel dispatch path and was rejected for $5 anti-drift and $7 layer-purity violations.

## OBJECTIVE
- Remove domain leakage from runtime/host/adapters
- Introduce resolver/registry pattern inside runtime
- Preserve `RuntimeControlPlane` + middleware pipeline
- DO NOT introduce new dispatch architecture

## CONSTRAINTS
- $5 Anti-drift: no new patterns beyond resolver/registry
- $7 Layer purity: domain layer must remain dependency-free (no `IServiceCollection` references)
- $6 File system: only operate in /src, /infrastructure, /tests, /docs, /scripts, /claude
- Existing `IProjectionHandler` (`Whyce.Shared.Contracts.Infrastructure.Projection`) must be reused, not shadowed
- `EventTypeResolver` direction is string→Type (event-store replay); must not be inverted
- All Todo commands in scope: Create, Update, **Complete** (the original patch omitted Complete)
- `Program.cs` Todo coupling is extensive (engine registry, workflow registry, schema registry, intent handler) — bootstrap module must address all of it, not just projection wiring

## EXECUTION STEPS (as proposed — see VALIDATION CRITERIA for delta)
1. Create `EventHandlerRegistry` in `src/runtime/dispatcher/`
2. Create `ProjectionHandlerRegistry` in `src/runtime/dispatcher/`
3. Replace `TodoProjectionBridge` with `GenericProjectionBridge`
4. Refactor `EventTypeResolver` to be schema-registry-driven
5. Refactor `KafkaProjectionConsumerWorker` to be generic (topic-list + handler-resolver + schema-registry + projection-table-resolver)
6. Move policy ID constants out of `runtime/policies/.../todo/` into `shared/contracts/`
7. Scrub all Todo coupling from `Program.cs`
8. Create bootstrap module **outside the domain layer** (location TBD: `src/systems/midstream/wss/composition/` or `src/platform/host/composition/`)
9. Add R-DOM-01 sub-clause under runtime.guard.md rule 11
10. dotnet build + integration test (POST/GET against /api/todo)

## OUTPUT FORMAT
- Phase A: prompt storage + guard update + new-rules capture (this execution)
- Phase B: code refactor (deferred — needs decisions on IProjectionHandler reuse, bootstrap module location, generic Kafka consumer scope, projection table resolver)
- Phase C: validation + audit sweep per $1b

## VALIDATION CRITERIA
- Phase A: 3 files created/updated (this prompt, runtime.guard.md, new-rules entry); zero source code touched
- Phase B (when executed):
  - `grep "Todo" src/runtime/` returns ZERO matches
  - `grep "Todo" src/platform/host/` returns ZERO matches
  - `grep "Todo" src/platform/host/adapters/` returns ZERO matches
  - Domain layer has zero `Microsoft.Extensions.DependencyInjection` references
  - `dotnet build` succeeds
  - POST /api/todo/create, /api/todo/update, /api/todo/complete and GET /api/todo/{id} all return success

## DELTA FROM ORIGINAL PATCH (rejection reasons)
1. Step 3 `IProjectionHandler` collides with existing contract — must reuse
2. Step 4 inverted method direction — would break event-store replay
3. Step 5 understated KafkaProjectionConsumerWorker complexity (topic, consumer group, schema mapping, Postgres write all hardcoded)
4. Step 6 misframes `TodoPolicyDefinition` as policy logic — it's 4 string identifiers
5. Step 7 Program.cs cleanup omits engine/workflow/schema/intent-handler wiring (~10 sites)
6. Step 8 places `TodoDomainModule` in `src/domain/` with `IServiceCollection` parameter — violates $7 layer purity
7. Step 9 R-DOM-01 duplicates existing rule 11 — should be sub-clause
8. Patch omits `CompleteTodoCommand` from sweep
