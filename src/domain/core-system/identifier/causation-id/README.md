# core-system / identifier / causation-id

**Classification:** core-system  
**Context:** identifier  
**Domain:** causation-id

## Purpose
A causal link that records which command or event caused the current message to be produced. Where `CorrelationId` groups a logical operation, `CausationId` expresses the direct parent-child relationship between messages.

## Scope
- `CausationId` value object: value (64-char lowercase hex string)
- Represents the ID of the immediately preceding command or event in the causal chain

## Does Not Own
- Causal graph construction or traversal (→ runtime / observability)
- Message routing (→ platform-system)

## Inputs
- Pre-computed SHA256 hex string (64 lowercase hex characters) — typically the ID of the causing command or event envelope

## Outputs
- `CausationId` value object — no domain events

## Invariants
- Value is exactly 64 lowercase hex characters
- Immutable once constructed
- CausationId is always assigned by the engine from the inbound message; never generated inside the domain

## Dependencies
- None
