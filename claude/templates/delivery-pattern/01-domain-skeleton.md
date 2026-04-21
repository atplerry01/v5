# 01 — Domain Skeleton

## Folder layout (CLASSIFICATION > CONTEXT > [DOMAIN-GROUP] > DOMAIN — three or four levels per DS-R3 / DS-R3a)

Per `domain.guard.md` rule 1 (CLASSIFICATION > CONTEXT > DOMAIN TOPOLOGY) and DS-R1 (`{classification}-system` suffix in `src/domain/` only):

```
src/domain/{classification}-system/
  {context}/
    {domain}/
      aggregate/
      entity/
      error/
      event/
      service/
      specification/
      value-object/
```

**Example (economic exemplar):** [src/domain/economic-system/capital/account/](../../../src/domain/economic-system/capital/account/)

## Artifact subfolders

Per `domain.guard.md` rule 2 (MANDATORY ARTIFACT FOLDERS), every BC contains the **MUST** subfolders and contains the **WHEN-NEEDED** subfolders only if the domain genuinely requires them. Omission of a WHEN-NEEDED folder is acceptable and must be recorded in the BC's README (one sentence: "no `entity/` — aggregate has no child entities"). Placeholder `.gitkeep` is acceptable for D0 BCs.

**MUST folders (always present):**

| Folder | Purpose | Naming |
|---|---|---|
| `aggregate/` | Aggregate root (sole entry point) | `{DomainConcept}Aggregate.cs` |
| `error/` | Domain-specific errors | `{Concept}Errors.cs` (static factory) |
| `event/` | Past-tense domain events | `{Subject}{PastTenseVerb}Event.cs` |
| `value-object/` | Immutable VOs | `{Concept}.cs` (record struct) |

**WHEN-NEEDED folders (present iff the domain requires them):**

| Folder | Present when | Naming |
|---|---|---|
| `entity/` | Aggregate owns non-VO child entities with their own identity | `{Concept}.cs` |
| `service/` | Stateless cross-aggregate coordination exists that cannot live on a single aggregate | `{Concept}Service.cs` |
| `specification/` | Composable boolean invariants need extraction from aggregate inline logic | `{Rule}Specification.cs` |

**Exemplar evidence:** in `src/domain/economic-system/` (42 BCs), roughly 1 in 5 BCs legitimately omits `entity/`, `service/`, or `specification/` because the domain does not require that artifact. Examples: `enforcement/{lock,restriction,sanction}` omit `entity/` and `service/`; `routing/path` omits `entity/`; `revenue/revenue` omits `specification/`. All are at or above D1.

## Naming rules (locked)

- **Aggregate**: `{DomainConcept}Aggregate` (e.g., `CapitalAccountAggregate`). Never bare `Account`.
- **Event**: past-tense `{Subject}{Verb}Event` (e.g., `CapitalAccountOpenedEvent`). Future tense forbidden.
- **Error**: domain-specific factory class `{Concept}Errors` returning `DomainException` / `DomainInvariantViolationException`.
- **Specification**: `{Rule}Specification` returning bool.
- **Value Object**: `{Concept}` (no suffix). Use `readonly record struct`.
- **No prefix noise**: Folder is `account/` not `capital-account/` (parent is already `capital/`).
- **Namespace**: `Whycespace.Domain.{System}.{Context}.{Domain}` (PascalCase, no hyphens).

## Aggregate pattern

Reference: [src/domain/economic-system/capital/account/aggregate/CapitalAccountAggregate.cs](../../../src/domain/economic-system/capital/account/aggregate/CapitalAccountAggregate.cs)

Required:

1. Inherits `AggregateRoot` from shared kernel.
2. Constructor private; static factory `Create(...)` or `Open(...)` returns the aggregate.
3. Lifecycle-init guard: `if (Version >= 0) throw <Aggregate>Errors.AlreadyInitialized();` per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
4. Every state mutation calls `RaiseDomainEvent(new {Subject}{Verb}Event(...))` per behavioral rule 15.
5. Apply methods are `private void Apply({Event}Event evt)` — pattern matched in base class.
6. No `DateTime.*`, `Guid.NewGuid()`, `Random` — per `constitutional.guard.md` GE-01.
7. No DI references (`Microsoft.Extensions.DependencyInjection.*`) — per D-PURITY-01.
8. No BCL exceptions — use `Guard.Against(...)` or `{Aggregate}Errors.Xxx()` per D-ERR-TYPING-01.

## Value Object pattern

Reference: [src/domain/economic-system/capital/account/value-object/AccountId.cs](../../../src/domain/economic-system/capital/account/value-object/AccountId.cs)

Canonical form (validated, single-primitive):

```csharp
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.{System}.{Context}.{Domain};

public readonly record struct {Concept}Id
{
    public Guid Value { get; }

    public {Concept}Id(Guid value)
    {
        Guard.Against(value == Guid.Empty, "{Concept}Id cannot be empty.");
        Value = value;
    }
}
```

Why explicit constructor instead of positional `record struct {Concept}Id(Guid Value)`:

- Allows validation per rule 5 (immutability) without losing it.
- Single-primitive `Value` / `Code` property name preserved so `WrappedPrimitiveValueObjectConverterFactory` in [EventDeserializer.cs](../../../src/runtime/event-fabric/EventDeserializer.cs) handles JSON automatically per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.
- `default(T)` still bypasses the constructor — known C# limitation; surface as runtime invariant violation if it escapes.

## Strongly-typed identifier discipline

Per `domain.guard.md` D-VO-TYPING-01: when a VO exists for an identifier, aggregate properties, event members, factory parameters, and service methods MUST use the VO type, not raw `Guid` or `string`.

Audit grep: `grep -E 'public Guid \w+Id|Guid \w+Id[,)]|string \w+Id[,)]' src/domain/{classification}-system/`. Any hit is an S2 (Guid) or S1 (string) violation.

## Event pattern

Reference: [src/domain/economic-system/vault/account/event/VaultAccountCreatedEvent.cs](../../../src/domain/economic-system/vault/account/event/VaultAccountCreatedEvent.cs)

```csharp
using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.{System}.{Context}.{Domain};

public sealed record {Subject}{Verb}Event(
    [property: JsonPropertyName("AggregateId")] {Subject}Id {Subject}Id,
    {OtherTypedField}) : DomainEvent;
```

The `[JsonPropertyName("AggregateId")]` attribute is required per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01` so STJ binds the schema-mapped JSONB key to the typed VO parameter on replay.

## Error pattern

Reference: [src/domain/economic-system/capital/account/error/CapitalAccountErrors.cs](../../../src/domain/economic-system/capital/account/error/CapitalAccountErrors.cs)

```csharp
namespace Whycespace.Domain.{System}.{Context}.{Domain};

public static class {Aggregate}Errors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("{Aggregate} has already been initialized.");

    public static DomainException InvalidAmount() =>
        new DomainInvariantViolationException("{Aggregate} requires a positive amount.");
}
```

No raw `throw new ArgumentException` etc. per D-ERR-TYPING-01.

## Specification pattern

Pure functions, no I/O, no DateTime, no DI. Composable predicates.

```csharp
public sealed class {Rule}Specification
{
    public bool IsSatisfiedBy({Aggregate} candidate) => /* pure boolean */;
}
```

## Cross-System Invariants (concept definition)

A **cross-system invariant** is a rule that spans **more than one bounded context** and must hold at all times across the platform. It is the sixth pillar of truth — added to structural, business, content, economic, and operational truth — and expresses **cross-system truth: consistency between all systems**.

### What cross-system invariants are

Pure decision rules that evaluate facts drawn from **multiple aggregates, references, or contexts** and return **Allow / Deny + reason**. They encode **domain-level policies that no single aggregate can enforce** because the authoritative facts live in separate contexts.

### What they are NOT

- **NOT aggregate invariants.** Aggregate invariants guard a single aggregate's internal state (balance ≥ 0, status transitions). Cross-system invariants span aggregates and do not live on any aggregate.
- **NOT projections.** Projections are derived read models, non-authoritative, eventually consistent. Invariants are authoritative and evaluated **before** an event is emitted.
- **NOT infrastructure.** They are not Kafka topics, not Postgres constraints, not retry policies. They carry domain meaning.
- **NOT OPA / authorization policies.** OPA answers "who may invoke this command?". Cross-system invariants answer "is the resulting state across contexts consistent?". OPA MUST NOT override a cross-system invariant per 06.10.
- **NOT duplicated aggregate logic.** If a rule fits inside a single aggregate, it belongs there. Promote to cross-system only when facts from two or more contexts are required.

### Canonical examples

| Invariant | Spans | Rule |
|---|---|---|
| `ContractEconomicPolicy` | business-system/agreement + economic-system | A **contract** must produce an attributable economic value (an economic subject must exist and be linked) before the contract is finalized. |
| `AmendmentContentPolicy` | business-system/agreement/change-control + content-system | An **amendment** must reference a valid content artifact (`DocumentRef` must resolve to an extant, non-revoked document). |
| `DocumentOwnershipPolicy` | content-system/document + business-system + structural-system | A **document** must bind to both a structural owner (hierarchy node) and a business owner (subject) before it becomes citable. |
| `SettlementLedgerPolicy` | economic-system/settlement + economic-system/ledger | A **settlement** must match the ledger state — the settlement amount equals the sum of unsettled journal entries for the target subject at the evaluation point. |

### Folder layout

Cross-system invariants live in a dedicated per-classification folder, **outside** any BC:

```
src/domain/{classification}-system/
  invariant/
    {policy-name}/
      {PolicyName}Policy.cs
      {PolicyName}Input.cs          — aggregate refs + context facts required
      {PolicyName}Decision.cs       — Allow | Deny(reason)
      {PolicyName}Specification.cs  — optional composable predicate
```

Rules:

1. The `invariant/` folder is at the **classification** level (sibling to `{context}/`), never inside a BC.
2. Policies take multiple aggregates or aggregate references as input and return a typed `PolicyDecision` (Allow / Deny + reason). No mutation, no I/O, no DateTime, no Guid.NewGuid().
3. Policies have **zero external dependencies** — pure functions like aggregates.
4. When a policy spans classifications (e.g., business-system ↔ economic-system), it lives under the **initiating** classification and references VOs/refs from the other classification through `src/shared/contracts/`.
5. The `{PolicyName}Input` record is the minimum fact-set required — never pass whole aggregates when a reference and a projection fact suffice.

### Naming rules

- Policy class: `{Concept}Policy` (e.g., `ContractEconomicPolicy`, `AmendmentContentPolicy`).
- Decision result: `PolicyDecision` (shared kernel type) or `{Concept}PolicyDecision` when a policy-specific reason taxonomy is needed.
- Specification: `{Concept}PolicySpecification` when decomposed into composable predicates.
- Never suffix with `Service`, `Rule`, `Check`, `Validator` — reserved for other constructs.

### Relationship to existing layers

| Layer | Owns | Example |
|---|---|---|
| Aggregate invariants | Intra-aggregate consistency | `CapitalAccountAggregate` rejects negative balance |
| **Cross-system invariants (this section)** | **Inter-context / inter-classification consistency** | **`ContractEconomicPolicy` rejects contract finalize when no economic subject exists** |
| Authorization policies (OPA) | Who may invoke a command | Only `treasurer` role may dispatch `SettleContractCommand` |
| Governance policies (OPA) | Cross-cutting organizational rules | Every command issued outside business hours requires second approval |

The four are complementary. All four must pass for a command to succeed.

## Anti-drift checks per BC

Run before declaring D2:

- [ ] All MUST subfolders present (`aggregate/`, `error/`, `event/`, `value-object/`).
- [ ] Each WHEN-NEEDED subfolder (`entity/`, `service/`, `specification/`) either present or justified-as-omitted in the BC README.
- [ ] No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, `Random` anywhere in BC.
- [ ] No `Microsoft.Extensions.DependencyInjection.*` imports.
- [ ] No raw BCL exception throws (D-ERR-TYPING-01).
- [ ] All identifiers use VO types per D-VO-TYPING-01.
- [ ] Aggregate has `Version >= 0` lifecycle-init guard.
- [ ] Every state mutation emits a past-tense event.
- [ ] Aggregate has a replay regression test if any event uses a wrapper-struct VO (per INV-REPLAY-LOSSLESS-VALUEOBJECT-01).
- [ ] Every command that spans contexts declares its required cross-system invariants in the BC README (rule, policy class, enforcement point). Commands that do not span contexts explicitly state "no cross-system invariants required".
