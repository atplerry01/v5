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

## Mandatory artifact subfolders

Per `domain.guard.md` rule 2 (MANDATORY ARTIFACT FOLDERS), every BC contains exactly these subfolders. Missing folders signal an incomplete BC. Placeholder `.gitkeep` is acceptable for D0 BCs.

| Folder | Purpose | Naming |
|---|---|---|
| `aggregate/` | Aggregate root (sole entry point) | `{DomainConcept}Aggregate.cs` |
| `entity/` | Entities owned by aggregate | `{Concept}.cs` |
| `error/` | Domain-specific errors | `{Concept}Errors.cs` (static factory) |
| `event/` | Past-tense domain events | `{Subject}{PastTenseVerb}Event.cs` |
| `service/` | Stateless domain services | `{Concept}Service.cs` |
| `specification/` | Pure boolean predicates | `{Rule}Specification.cs` |
| `value-object/` | Immutable VOs | `{Concept}.cs` (record struct) |

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

## Anti-drift checks per BC

Run before declaring D2:

- [ ] Folder structure matches the 7 mandatory subfolders.
- [ ] No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, `Random` anywhere in BC.
- [ ] No `Microsoft.Extensions.DependencyInjection.*` imports.
- [ ] No raw BCL exception throws (D-ERR-TYPING-01).
- [ ] All identifiers use VO types per D-VO-TYPING-01.
- [ ] Aggregate has `Version >= 0` lifecycle-init guard.
- [ ] Every state mutation emits a past-tense event.
- [ ] Aggregate has a replay regression test if any event uses a wrapper-struct VO (per INV-REPLAY-LOSSLESS-VALUEOBJECT-01).
