# 02 — Engine Skeleton

## Engine tier model

| Tier | Role | When to use |
|---|---|---|
| **T0U** | Determinism, HSID, sequencing | Foundational — single instance for the platform |
| **T1M** | Multi-step workflows with compensation | Cross-step state machines (saga patterns) |
| **T2E** | Single-shot command handlers | One-command-one-aggregate-mutation |
| **T3I** | Integration / inbound systems | Excluded from Phase 2 per [phase-2b.md](../../project-topics/v2b/phase-2b.md) line 26 |
| **T4A** | Analytics / outbound systems | Phase 3+ |

For a typical EX vertical, you implement **T1M** (workflow orchestration) and/or **T2E** (single-shot handlers). The decision: does the command require multiple steps with compensation? T1M. Otherwise T2E.

## T1M layout (workflow engine)

Reference: [src/engines/T1M/domains/economic/transaction/](../../../src/engines/T1M/domains/economic/transaction/)

```
src/engines/T1M/domains/{classification}/
  {context}/
    handlers/         — workflow handlers (entry points)
    pipeline/         — pipeline steps definitions
    steps/            — atomic step implementations
    workflows/        — workflow state machine definitions
```

### Step pattern

Reference: [src/engines/T1M/domains/economic/transaction/steps/PostToLedgerStep.cs](../../../src/engines/T1M/domains/economic/transaction/steps/PostToLedgerStep.cs)

Required attributes per step:

1. Implements `IWorkflowStep<TInput, TOutput>`.
2. Pure command dispatch via `ISystemIntentDispatcher` — no infrastructure access.
3. Records OpenTelemetry metrics (`stepsExecuted`, `stepsFailed`, latency histogram).
4. Returns `RuntimeResult<TOutput>` — never throws across step boundary (engines translate to results per behavioral rule 12).
5. Idempotent — re-execution of the same step with the same inputs produces the same outputs.
6. Includes retry / recovery escalation paths if the step crosses an external boundary (ledger, payment provider, etc.).
7. No `DateTime.*`, `Guid.NewGuid()`, `Random` per `constitutional.guard.md` GE-01.

### Workflow pattern

Reference: [src/engines/T1M/domains/economic/transaction/workflows/](../../../src/engines/T1M/domains/economic/transaction/workflows/)

Each workflow:

- Declares its steps in execution order.
- Declares compensation steps for each forward step.
- Records terminal state (`Completed`, `Failed`, `Compensated`).
- Emits a `{Workflow}CompletedEvent` or `{Workflow}FailedEvent` to the event store.
- Is recoverable: can resume from any step on host restart.

## T2E layout (single-shot handlers)

Reference: [src/engines/T2E/economic/](../../../src/engines/T2E/economic/)

```
src/engines/T2E/{classification}/
  {context}/
    {domain}/
      {Action}{Aggregate}Handler.cs
```

### T2E handler pattern

Reference: [src/engines/T2E/economic/vault/account/InvestHandler.cs](../../../src/engines/T2E/economic/vault/account/InvestHandler.cs)

Required:

1. Single command → single aggregate → emits 1+ events. Per behavioral rule 10 (AGGREGATE TRANSACTION BOUNDARY).
2. Loads aggregate via `IAggregateRepository<T>`.
3. Calls aggregate behavior method with VO-typed parameters per D-VO-TYPING-01.
4. Saves via repository — repository handles outbox publish per `runtime.guard.md`.
5. No direct Kafka / SQL / HTTP — engine MUST NOT call infrastructure (S0 violation per behavioral guard).
6. No policy evaluation — assumes runtime middleware already authorized per behavioral rule 13.

## Engine wiring (composition)

Reference: [src/platform/host/composition/economic/EconomicCompositionRoot.cs](../../../src/platform/host/composition/economic/EconomicCompositionRoot.cs)

Each vertical's composition root:

- Registers all T1M workflows + steps via `services.AddT1M{Vertical}Workflows()`.
- Registers all T2E handlers via `services.AddT2E{Vertical}Handlers()`.
- Registers domain repositories via `services.AddDomain{Vertical}Repositories()`.
- Is added to `BootstrapModuleCatalog.All` so the host picks it up at startup.

## Engine forbidden list

Per behavioral guard rules 1–4 (S0):

- ❌ NO direct DB access (`DbContext`, `IDbConnection`, `SqlCommand`).
- ❌ NO event publishing (`eventBus.Publish()`, `IEventPublisher`, Kafka produce).
- ❌ NO infra calls (Redis, HTTP, file I/O).
- ❌ NO engine-to-engine references (T2E cannot import T1M).
- ❌ NO policy evaluation (`if (user.HasRole(...))`).

## Engine quality gate

Run before declaring engine portion of the vertical D2:

- [ ] Every command in the vertical has a corresponding T1M workflow OR T2E handler.
- [ ] Zero `NotImplementedException`.
- [ ] Zero `// TODO`, `// FIXME`, `// HACK` comments.
- [ ] Zero direct DB / Kafka / HTTP calls (grep verifies).
- [ ] Every handler returns `RuntimeResult<T>` — no exception leakage.
- [ ] Every step records OTel metrics.
- [ ] Steps are < 300 LOC; if larger, decompose.
