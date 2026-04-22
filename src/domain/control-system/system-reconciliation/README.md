# system-reconciliation

**Classification:** control-system
**Context:** system-reconciliation

## Purpose

Provides the domain model for system-level reconciliation operations — the automated and auditable process of detecting, classifying, and resolving discrepancies between expected and actual system state. This context is distinct from business-level reconciliation; it governs platform consistency as a control-system concern.

## Domains

| Domain | Aggregate | Responsibility |
|---|---|---|
| consistency-check | ConsistencyCheckAggregate | Scoped evaluation of system state consistency |
| discrepancy-detection | DiscrepancyDetectionAggregate | Identification and classification of detected discrepancies |
| discrepancy-resolution | DiscrepancyResolutionAggregate | Lifecycle management for resolving confirmed discrepancies |
| reconciliation-run | ReconciliationRunAggregate | Bounded execution of the full reconciliation pipeline |
| system-verification | SystemVerificationAggregate | Targeted integrity verification of a named system |

## Activation

All domains are at **D2 (active)** — full aggregate, events, value objects, and error definitions are present.
