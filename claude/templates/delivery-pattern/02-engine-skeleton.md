# 02 — Engine Skeleton

## Engine tier model

| Tier | Role | When to use |
|---|---|---|
| **T0U** | Determinism, HSID, sequencing | Foundational — single instance for the platform |
| **T1M** | Multi-step workflows with compensation | Cross-step state machines (saga patterns) |
| **T2E** | Single-shot command handlers | One-command-one-aggregate-mutation |
| **T3I** | Integration / inbound systems | Excluded from Phase 2 per [phase-2b.md](../../project-topics/v2b/phase-2b.md) line 26 |
| **T4A** | Analytics / outbound systems | Phase 3+ |

For a typical EX vertical, you implement **T2E** everywhere commands mutate a single aggregate, and **T1M** only where multi-step orchestration with compensation is required. **T1M is not universal** — in the economic exemplar only 3 of 13 contexts have T1M (`transaction`, `enforcement`, `revenue`). The decision: does the command require multiple steps with compensation? T1M. Otherwise T2E.

## T1M layout (workflow engine)

Reference: [src/engines/T1M/domains/economic/transaction/](../../../src/engines/T1M/domains/economic/transaction/)

```
src/engines/T1M/domains/{classification}/
  {context}/
    state/            — workflow state machine definition (one {Workflow}WorkflowState.cs per workflow)
    steps/            — atomic step implementations
```

Notes:

- There is no `handlers/`, `pipeline/`, or `workflows/` subfolder. The workflow state machine is a single `{Workflow}WorkflowState.cs` file under `state/` that enumerates the steps, terminal states, and compensation pairings declaratively.
- Some contexts have multiple workflows — they co-exist under one `state/` folder (e.g., economic/revenue contains both `DistributionWorkflowState.cs` and `DistributionCompensationWorkflowState.cs`).
- Some contexts add a 3rd level (e.g., `T1M/domains/economic/revenue/distribution/state/`) when multiple distinct sub-workflows belong to a single BC. Nesting is per-need, not universal.

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

### Workflow state pattern

Reference: [src/engines/T1M/domains/economic/transaction/state/TransactionLifecycleWorkflowState.cs](../../../src/engines/T1M/domains/economic/transaction/state/TransactionLifecycleWorkflowState.cs)

Each `{Workflow}WorkflowState.cs` declares:

- Ordered forward steps.
- Compensation step for each forward step (so the engine can unwind on failure).
- Terminal states (`Completed`, `Failed`, `Compensated`).
- Emission of a terminal event (`{Workflow}CompletedEvent` / `{Workflow}FailedEvent`) to the event store.
- Recoverability — resumable from any step on host restart via state rehydration.

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

## Cross-System Invariants (enforcement mechanism)

Cross-system invariants (defined in [01-domain-skeleton.md](01-domain-skeleton.md) § Cross-System Invariants) are **evaluated inside the engine**, **before** the aggregate is mutated. The engine is the only layer that can load multiple aggregates / refs and call a domain policy; the domain layer cannot because policies span contexts.

### Domain Policy Layer (pure decision functions)

The classification-level folder `src/domain/{classification}-system/invariant/{policy-name}/` holds **pure decision functions**:

- **Inputs:** one or more aggregates, aggregate references, or projection-derived fact records.
- **Output:** `PolicyDecision` = `Allow` | `Deny(reason)`.
- **Constraints:** no mutation, no I/O, no `DateTime.*`, no `Guid.NewGuid()`, no DI, no BCL exceptions. Same discipline as aggregates (per `domain.guard.md` $7 and D-PURITY-01).

```csharp
namespace Whycespace.Domain.{System}.Invariant.{Policy};

public sealed class {Concept}Policy
{
    public PolicyDecision Evaluate({Concept}PolicyInput input) =>
        /* pure boolean/reasoned decision across input.AggregateA, input.AggregateB, input.ProjectionFact */
        input.IsSatisfied
            ? PolicyDecision.Allow()
            : PolicyDecision.Deny({Concept}PolicyReason.MissingEconomicSubject);
}
```

### Enforcement points

Cross-system invariants MUST be enforced at one of two points per command, never both:

| Pattern | Where | When to use |
|---|---|---|
| **Inline in T2E handler** | `src/engines/T2E/{cls}/{ctx}/{dom}/{Action}Handler.cs` | Simple cases: single command, one invariant, facts loadable in a single step |
| **As a workflow step in T1M** | `src/engines/T1M/domains/{cls}/{ctx}/steps/Evaluate{Policy}Step.cs` | Multi-step cases: invariant depends on prior step output, or failure requires compensation |

### Pattern: T2E with inline invariant

```csharp
public sealed class FinalizeContractHandler
{
    private readonly IAggregateRepository<ContractAggregate> _contracts;
    private readonly IAggregateRepository<EconomicSubjectAggregate> _subjects;
    private readonly ContractEconomicPolicy _policy;

    public async Task<RuntimeResult<Unit>> HandleAsync(FinalizeContractCommand cmd, CancellationToken ct)
    {
        var contract = await _contracts.LoadAsync(cmd.ContractId, ct);
        var subject = await _subjects.LoadAsync(cmd.EconomicSubjectId, ct);

        var decision = _policy.Evaluate(new ContractEconomicPolicyInput(contract, subject));
        if (decision.IsDeny)
            return RuntimeResult<Unit>.Failure(decision.Reason);

        contract.Finalize();
        await _contracts.SaveAsync(contract, ct);
        return RuntimeResult<Unit>.Success();
    }
}
```

Rules:

1. **Load required aggregates.** Via `IAggregateRepository<T>` only — never direct DB.
2. **Evaluate policy.** Call the injected `{Concept}Policy` with a typed input record.
3. **Fail fast.** On `Deny`, return `RuntimeResult.Failure(reason)` **before** any aggregate mutation. No event is emitted.
4. **Proceed if Allow.** Mutate the aggregate and save — the policy has proven cross-system consistency at emission time.
5. **No retry of a `Deny` decision.** A deny is a domain truth, not a transient failure. It does not enter retry / DLQ paths.

### Pattern: T1M workflow step

For multi-step flows, extract the evaluation into a dedicated step:

```
src/engines/T1M/domains/{cls}/{ctx}/steps/
  Evaluate{Concept}PolicyStep.cs
```

The step:

1. Loads the required aggregates via repositories injected into the step.
2. Calls the injected `{Concept}Policy.Evaluate(...)`.
3. Returns `RuntimeResult<{Concept}PolicyDecision>`.
4. On `Deny`, the workflow transitions to a terminal `PolicyRejected` state — no compensation of prior steps needed because no aggregate was mutated by the policy step.
5. On `Allow`, workflow continues to the next step.

The workflow state declares `EvaluateXxxPolicyStep` as an early step, positioned **before** any mutating step so that a `Deny` unwinds nothing.

### Injection

Domain policies are registered in the composition root as singletons (they are pure and stateless) and injected into handlers and steps. See [03-runtime-wiring.md](03-runtime-wiring.md) § Policy registration for registration detail.

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
- [ ] Every command that declares cross-system invariants (per [01-domain-skeleton.md](01-domain-skeleton.md) § Cross-System Invariants, recorded in the BC README) has a matching enforcement point — inline in its T2E handler OR as an `Evaluate{Concept}PolicyStep` in its T1M workflow.
- [ ] Enforcement occurs **before** aggregate mutation; a `Deny` decision returns `RuntimeResult.Failure` without emitting any event.
