# core-system / identifier / correlation-id

**Classification:** core-system  
**Context:** identifier  
**Domain:** correlation-id

## Purpose
A cross-boundary correlation key that links related commands and events within a logical operation (e.g., a user request, a saga). All messages produced in response to the same root trigger share the same `CorrelationId`.

## Scope
- `CorrelationId` value object: value (64-char lowercase hex string)
- Propagation contract: a correlation ID is passed through from inbound command to all outbound events and downstream commands

## Does Not Own
- Correlation tracking or storage (→ runtime)
- Saga or process management (→ orchestration layer)

## Inputs
- Pre-computed SHA256 hex string (64 lowercase hex characters)

## Outputs
- `CorrelationId` value object — no domain events

## Invariants
- Value is exactly 64 lowercase hex characters
- Immutable once constructed
- CorrelationId is never generated inside the domain — it is always passed in from the command envelope

## Dependencies
- None
