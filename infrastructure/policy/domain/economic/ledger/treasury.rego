package whyce.policy.economic.ledger.treasury

# ──────────────────────────────────────────────────────────────────────
# Phase 7 T7.12 — Treasury policy (FINANCIAL-GUARDRAIL layer:
# "what is allowed financially")
#
# Companion to obligation.rego (NORMATIVE layer). Obligation governs
# whether settlement *must* occur; treasury governs whether the
# corresponding movement of funds is financially *allowed*. Neither
# reinterprets the other's semantics — each reinforces its own
# aggregate's invariants at the authorization boundary.
#
# Invariants reinforced (every rule mirrors a TreasuryAggregate
# Guard.Against / throw site — no rule invents a new one):
#
#   Create         — TreasuryAggregate.Create
#     * no numeric invariants (balance starts at 0)
#     * currency must be non-empty (contract-level)
#
#   AllocateFunds  — TreasuryAggregate.AllocateFunds
#     * amount > 0             (TreasuryErrors.InvalidAmount)
#     * amount <= Balance      (TreasuryErrors.InsufficientTreasuryFunds)
#
#   ReleaseFunds   — TreasuryAggregate.ReleaseFunds
#     * amount > 0             (TreasuryErrors.InvalidAmount)
#
# Balance-invariant reinforcement:
#   When `input.resource.state.balance` is supplied, AllocateFunds is
#   refused up-front if amount > balance. This mirrors the aggregate's
#   InsufficientTreasuryFunds guard at the authorization boundary so
#   the decision trail records a clean deny reason rather than a
#   domain exception surfacing mid-pipeline.
#
# Determinism contract:
#   * NO data.* lookups against dynamic sources
#   * NO http.send, time.now_ns, or rand
#   * Every decision is a pure function of `input`
#   * Non-deterministic inputs (balance snapshot, wall-clock) MUST
#     arrive explicitly via `input.resource.state.*` / `input.now_ns`
#     — the policy never reads them from a built-in side effect
#
# Replay-safety: identical to obligation.rego. The same `input` always
# produces the same verdict; decisions can be re-evaluated against
# recorded audit trails deterministically.
#
# Backward-compatibility contract:
#   * `input.command` and `input.resource.state` are OPTIONAL. When
#     absent, rules fall through to role-based allow; the aggregate
#     still catches any violation. When PRESENT, the tighter
#     state-aware checks fire and reject at the policy boundary.
# ──────────────────────────────────────────────────────────────────────

import rego.v1

default allow := false

# ── Common structural guard ───────────────────────────────────────────

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "treasury"
}

# ── whyce.economic.ledger.treasury.create ─────────────────────────────
# Operator or admin may create a new treasury. No balance-sensitive
# invariants at creation (balance starts at zero); the only command-
# level reinforcement is non-empty currency.

allow if {
    input.policy_id == "whyce.economic.ledger.treasury.create"
    input.subject.role in {"operator", "admin"}
    valid_resource
    create_preconditions
}

create_preconditions if { not input.command }
create_preconditions if { input.command.currency != "" }

# ── whyce.economic.ledger.treasury.allocate_funds ─────────────────────
# Operator or admin may allocate. When the command is supplied, amount
# must be positive and (when balance state is supplied) must not exceed
# the current balance — mirrors AllocateFunds guards.

allow if {
    input.policy_id == "whyce.economic.ledger.treasury.allocate_funds"
    input.subject.role in {"operator", "admin"}
    valid_resource
    allocation_preconditions
}

allocation_preconditions if { not input.command }
allocation_preconditions if {
    input.command.amount > 0
    allocation_balance_ok
}

allocation_balance_ok if { not input.resource.state }
allocation_balance_ok if {
    input.command.amount <= input.resource.state.balance
}

# ── whyce.economic.ledger.treasury.release_funds ──────────────────────
# Admin-only: release augments the balance from an external funding
# source; the aggregate accepts any positive amount. Admin-gating here
# is a policy choice (not a domain invariant) to prevent unauthorised
# external-source attribution — retained from the pre-T7.12 baseline.

allow if {
    input.policy_id == "whyce.economic.ledger.treasury.release_funds"
    input.subject.role == "admin"
    valid_resource
    release_preconditions
}

release_preconditions if { not input.command }
release_preconditions if { input.command.amount > 0 }

# ── Structural deny rules ─────────────────────────────────────────────

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }

# ── State-aware deny reasons ──────────────────────────────────────────
# Fires when input.resource.state / input.command IS supplied. Each
# reason mirrors a specific TreasuryAggregate guard so the denial
# message remains auditable and aligned with the domain error surface.

deny_reason contains "treasury_insufficient_funds" if {
    input.policy_id == "whyce.economic.ledger.treasury.allocate_funds"
    input.command.amount > input.resource.state.balance
}

deny_reason contains "treasury_non_positive_amount" if {
    input.policy_id == "whyce.economic.ledger.treasury.allocate_funds"
    input.command.amount <= 0
}

deny_reason contains "treasury_non_positive_amount" if {
    input.policy_id == "whyce.economic.ledger.treasury.release_funds"
    input.command.amount <= 0
}

deny_reason contains "treasury_currency_missing" if {
    input.policy_id == "whyce.economic.ledger.treasury.create"
    input.command.currency == ""
}
