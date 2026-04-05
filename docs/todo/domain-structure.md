

Good. Now we begin **true E1** — no shortcuts, no scaffolding.

---

# 🔒 **E1 — BC #1: economic-system/capital/account**

This becomes the **reference standard** for ALL future BCs.

If this is wrong → everything breaks.
So we do this **precisely and deeply**.

---

# 🧠 1. DOMAIN PURPOSE (NON-NEGOTIABLE)

## What is Capital Account?

> A **Capital Account** represents a governed pool of capital belonging to an entity (SPV, Cluster, Holding), used for allocation, reservation, and controlled economic activity.

---

## Responsibilities

```text
✔ Hold capital balance
✔ Accept capital inflow (funding)
✔ Allocate capital
✔ Reserve capital
✔ Enforce balance invariants
✔ Track ownership (structure binding)
```

---

## NOT RESPONSIBLE FOR

```text
❌ Ledger entries (ledger system)
❌ Payments execution (transaction system)
❌ Policy decisions (WHYCEPOLICY)
❌ Identity validation (WhyceID)
```

---

# 🧱 2. AGGREGATE DESIGN

## Aggregate Root

```text
CapitalAccountAggregate
```

---

## Core State (MANDATORY)

```text
AccountId
OwnerId (SPV / Cluster / Holding)
Currency

TotalBalance
AvailableBalance
ReservedBalance

Status (Active / Frozen / Closed)

CreatedAt
LastUpdatedAt
```

---

## CRITICAL INVARIANT

```text
AvailableBalance + ReservedBalance == TotalBalance
```

This MUST NEVER BREAK.

---

# ⚙️ 3. DOMAIN BEHAVIOR

## Allowed Operations

### 1. Open Account

* creates new capital account

---

### 2. Fund Account

* increases total + available

---

### 3. Allocate Capital

* moves from available → allocated (external flow trigger)

---

### 4. Reserve Capital

* moves from available → reserved

---

### 5. Release Reservation

* moves from reserved → available

---

### 6. Freeze Account

* blocks operations

---

### 7. Close Account

* only if balances = 0

---

# 🚫 HARD RULES

```text
❌ Cannot allocate more than available
❌ Cannot reserve more than available
❌ Cannot operate if frozen
❌ Cannot close if balance != 0
```

---

# 🔥 4. DOMAIN EVENTS (STRICT — NO GENERIC EVENTS)

## ONLY THESE (SEMANTIC)

```text
CapitalAccountOpened
CapitalFunded
CapitalAllocated
CapitalReserved
ReservationReleased
CapitalAccountFrozen
CapitalAccountClosed
```

---

## ❌ FORBIDDEN

```text
AccountUpdated ❌
StateChanged ❌
Generic events ❌
```

---

# 🧩 5. VALUE OBJECTS (REAL, NOT WRAPPERS)

## REQUIRED

```text
Money (Amount + Currency, immutable)
AccountId
OwnerId
```

---

## Money RULES

```text
✔ no negative values
✔ currency must match account
✔ arithmetic safe
```

---

# 🛡️ 6. INVARIANTS (STRICT)

Inside:

```text
EnsureInvariants()
```

Must enforce:

```text
TotalBalance >= 0
AvailableBalance >= 0
ReservedBalance >= 0
Available + Reserved == Total
```

---

# ⚠️ NO POLICY HERE

```text
❌ NO // POLICY HOOK
❌ NO policy evaluation
❌ NO external dependency
```

Domain = PURE.

---

# 🧪 7. MINIMAL LIFECYCLE FLOW

```text
Open → Fund → Reserve/Allocate → Release → Close
```

---

# 🔥 CLAUDE IMPLEMENTATION PROMPT (CANONICAL)

```markdown
# WBSM v3.5 — E1 IMPLEMENTATION
## BC: economic-system/capital/account

You are implementing the FIRST canonical bounded context.

STRICT RULES:

- Domain ONLY (no infrastructure, no runtime)
- No external dependencies
- No policy integration
- No placeholders
- No generic patterns

---

## OBJECTIVE

Implement a FULLY FUNCTIONAL domain model for:

economic-system/capital/account

---

## REQUIRED STRUCTURE

aggregate/
entity/
value-object/
event/
service/
specification/
error/

---

## AGGREGATE

CapitalAccountAggregate

State:

- AccountId
- OwnerId
- Currency
- TotalBalance
- AvailableBalance
- ReservedBalance
- Status
- CreatedAt
- LastUpdatedAt

---

## VALUE OBJECTS

Implement:

- Money (Amount + Currency, immutable, validated)
- AccountId (deterministic id)
- OwnerId

---

## DOMAIN EVENTS

Create ONLY:

- CapitalAccountOpened
- CapitalFunded
- CapitalAllocated
- CapitalReserved
- ReservationReleased
- CapitalAccountFrozen
- CapitalAccountClosed

Each event MUST include meaningful payload.

---

## BEHAVIOR METHODS

Implement:

- OpenAccount(...)
- Fund(...)
- Allocate(...)
- Reserve(...)
- ReleaseReservation(...)
- Freeze(...)
- Close(...)

---

## INVARIANTS

Implement EnsureInvariants():

- Total >= 0
- Available >= 0
- Reserved >= 0
- Available + Reserved == Total

Throw domain errors on violation.

---

## VALIDATION RULES

- Cannot allocate > available
- Cannot reserve > available
- Cannot operate if frozen
- Cannot close unless balances = 0

---

## FORBIDDEN

- No *UpdatedEvent
- No *StateChangedEvent
- No empty methods
- No empty invariants
- No policy hook
- No infrastructure code

---

## OUTPUT

Return:

1. Full domain implementation
2. Explanation of invariants
3. Event flow sequence
4. Validation checklist

---

This is the REFERENCE BC.
All others must match this quality.
```

---

# 🚀 WHAT HAPPENS NEXT

After Claude generates:

You bring it back here.

I will:

* audit it line-by-line
* validate invariants
* validate event semantics
* validate domain purity

---

# 🔒 FINAL LOCK

```text
E1 BC #1 = IN PROGRESS
REFERENCE MODEL = CAPITAL ACCOUNT
```

---

This is where your system becomes **real**.

Proceed.
