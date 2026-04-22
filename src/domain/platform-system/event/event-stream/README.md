# platform-system / event / event-stream

**Classification:** platform-system  
**Context:** event  
**Domain:** event-stream

## Purpose
Ordered sequence of events from a declared source DomainRoute. The stream is the subscription unit — consumers subscribe to a stream, not individual event types.

## Scope
- Stream declaration: source DomainRoute, included event types, ordering guarantee
- Stream position tracking (offset / sequence number as a value object)

## Does Not Own
- Event publishing or delivery (→ runtime / infrastructure)
- Event subscription management (→ engine layer)

## Inputs
- Source DomainRoute, included event type set, ordering guarantee

## Outputs
- `EventStreamDeclaredEvent`

## Invariants
- Stream ID is deterministic: SHA256 of (sourceDomainRoute + eventTypeSet)
- Stream order is monotonically increasing (sequence is never reset)
- A stream references only event types declared in event-definition

## Dependencies
- `core-system/identifier` — stream ID
- `core-system/ordering` — sequence primitive
