# WHYCE ID DOCTRINE v4

## Overview

Whycespace uses a standardized deterministic ID format across the entire system.

All IDs MUST follow:

```
DOMAIN-TYPE-JURISDICTION-TIME-SUFFIX
```

Example:

```
HUM-PRT-UK-20260322153045123-A92F1B
DOC-EVT-NG-20260322153100211-B81D09
ECO-INC-UK-20260322153211888-C11F22
```

---

## Structure

### DOMAIN

3-letter codes:

| Code | Meaning |
|------|---------|
| HUM | Human Capital |
| ECO | Economic |
| IDN | Identity |
| CTR | Contract |
| DOC | Document |
| OPR | Operational |
| WRK | Workflow |
| EVT | Event |
| OBS | Observability |
| SYS | System |
| DAT | Data |
| GOV | Governance |

---

### TYPE

| Code | Meaning |
|------|---------|
| EVT | Event |
| CMD | Command |
| WRK | Workflow |
| PRT | Participant |
| OPR | Operator |
| ASN | Assignment |
| INC | Incentive |
| PER | Performance |
| REP | Reputation |
| ELG | Eligibility |
| SAN | Sanction |
| POL | Policy |
| DOC | Document |

---

### JURISDICTION

```
UK
NG
```

---

### TIME

Format:

```
yyyyMMddHHmmssfff
```

Example:

```
20260322153045123
```

This ensures sortable ordering within the same domain/type.

---

### SUFFIX

- 6 characters
- Deterministic (hash from DeterministicKey) — ALWAYS required

---

## Generation Rules

1. **ZERO `Guid.NewGuid()` in production code** — enforced by DeterminismGuardTests
2. ALL IDs MUST use `WhyceIdGenerator` or `DeterministicIdHelper.FromSeed(seed)`
3. DOMAIN and TYPE must use constants
4. TIME must use `IClock` (no `DateTime.Now`)
5. `DeterministicKey` is REQUIRED (not optional) — ensures same ID for same input
6. Seed MUST include: commandId OR eventId, entity type, and stable attributes

---

## Usage Example

```csharp
var id = generator.Generate(new IdGenerationContext
{
    Domain = IdDomain.HUM,
    Type = IdType.PRT,
    Jurisdiction = "UK",
    Timestamp = clock.UtcNow,
    DeterministicKey = "user-123"
});
```

---

## Applies To

- Aggregate IDs
- Entity IDs
- Event IDs
- Command IDs
- Workflow IDs

---

## Violations

The following are NOT allowed:

- Manual ID creation
- Direct GUID usage
- Incorrect format
- Missing domain/type

---

## Sorting

IDs are sortable within:

- Same domain
- Same type
- Same jurisdiction

For global ordering, use Timestamp separately.

---

## Final Rule

**ALL IDs IN WHYCESPACE MUST FOLLOW THIS FORMAT.**

**NO EXCEPTIONS.**
