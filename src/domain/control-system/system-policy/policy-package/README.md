# policy-package

**Classification:** control-system
**Context:** system-policy
**Domain:** policy-package

## Purpose

Groups related policy definitions into a named, versioned, deployable unit. A package provides a cohesive governance boundary for policies that must be deployed and retired together — ensuring consistency across enforcement points.

## Lifecycle

`Draft` → `Assembled` → `Deployed` → `Retired`

Assembly requires at least one policy definition. Deployment requires prior assembly. Retirement is final.

## Aggregate: PolicyPackageAggregate

| Property | Type | Description |
|---|---|---|
| Id | PolicyPackageId | Deterministic 64-hex SHA256 identifier |
| Name | string | Human-readable package name |
| Version | PackageVersion | Semantic version (major.minor) |
| PolicyDefinitionIds | IReadOnlySet&lt;string&gt; | Policies contained in this package |
| Status | PolicyPackageStatus | Draft / Assembled / Deployed / Retired |

## Invariants

- Name must not be empty.
- Package must contain at least one policy definition reference.
- Deployment requires Assembled status.
- Retired packages cannot be re-deployed.

## Events

| Event | Trigger |
|---|---|
| PolicyPackageAssembledEvent | Package composed with policy definitions |
| PolicyPackageDeployedEvent | Package activated for enforcement |
| PolicyPackageRetiredEvent | Package withdrawn from enforcement |
