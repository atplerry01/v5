# TITLE
E1→EX Full D2 Delivery — Streaming Context 5 Remaining BCs (Completion)

# CONTEXT
Continuation of prior session (context exhausted). Five streaming-context BCs were at D0 scaffold and required full E1→EX D2 delivery:
1. `delivery-governance/entitlement-hook`
2. `delivery-governance/moderation`
3. `live-streaming/ingest-session`
4. `playback-consumption/progress`
5. `playback-consumption/replay`

Prior session had written all domain, engine, runtime, projection, API, application module, Kafka, and SQL files. This session completed:
- `ContentSystemCompositionRoot.cs` — RegisterSchema, RegisterProjections, RegisterEngines for all 5 BCs
- `DomainSchemaCatalog.cs` — 5 new static register methods (done prior session)
- Fixed 21 engine handlers: `context.LoadAggregate(id)` → `await context.LoadAggregateAsync(typeof(...))`
- Fixed 5 controllers: `_idGenerator.NewId()` → `_idGenerator.Generate(seed)` with natural-key seeds

# OBJECTIVE
Bring all 5 streaming D0 BCs to full D2 (domain + engine + runtime + API + projection + infra + composition wiring), verified by `dotnet build` passing 0 errors.

# CONSTRAINTS
- WBSM v3 canonical execution rules apply
- Domain layer purity: zero external dependencies
- Deterministic IDs: IIdGenerator.Generate(seed) not Guid.NewGuid
- No DateTime.UtcNow in domain (IClock only) — engine factory handlers violate this; captured in new-rules
- LoadAggregateAsync(typeof(T)) — not LoadAggregate(id)

# EXECUTION STEPS
1. Read ContentSystemCompositionRoot.cs current state
2. Add 5 DomainSchemaCatalog calls in RegisterSchema
3. Add 5 × N event registrations in RegisterProjections (26 total events)
4. Add 5 ApplicationModule.RegisterEngines calls in RegisterEngines
5. Run dotnet build → 26 errors (LoadAggregate + NewId)
6. Fix 21 engine handlers: LoadAggregate → await LoadAggregateAsync(typeof(...))
7. Fix 5 controllers: NewId() → Generate(natural-key-seed)
8. Run dotnet build → 0 errors

# OUTPUT FORMAT
Build succeeded, 0 errors, 1 warning.

# VALIDATION CRITERIA
- dotnet build: 0 errors
- All 5 BCs registered in RegisterSchema, RegisterProjections, RegisterEngines
- All engine handlers use async/await pattern with LoadAggregateAsync
- All controller factory actions use IIdGenerator.Generate(seed)
