package whyce.policy.economic.ledger.obligation

# ──────────────────────────────────────────────────────────────────────
# Phase 7 T7.12 — Obligation policy (NORMATIVE layer: "what must happen")
#
# Companion to treasury.rego (FINANCIAL-GUARDRAIL layer). Together they
# bracket the ledger governance surface: obligation governs whether a
# settlement *must* occur between counterparties; treasury governs
# whether the movement of funds is financially *allowed*. Neither
# reinterprets the other's semantics — each reinforces its own
# aggregate's invariants at the authorization boundary.
#
# Invariants reinforced (every rule mirrors an ObligationAggregate
# Guard.Against / throw site — no rule invents a new one):
#
#   Create  — ObligationAggregate.Create
#     * amount > 0              (ObligationErrors.InvalidAmount)
#     * counterpartyId non-empty (ObligationErrors.InvalidCounterparty)
#     * type ∈ {Payable, Receivable}
#
#   Fulfil  — ObligationAggregate.Fulfil
#     * Status must be Pending  (ObligationErrors.ObligationNotPending,
#                                ObligationAlreadyFulfilled,
#                                CannotFulfilCancelledObligation)
#
#   Cancel  — ObligationAggregate.Cancel
#     * Status must be Pending  (ObligationErrors.ObligationNotPending,
#                                ObligationAlreadyCancelled,
#                                CannotCancelFulfilledObligation)
#
# Determinism contract:
#   * NO data.* lookups against dynamic sources
#   * NO http.send, time.now_ns, or rand
#   * Every decision is a pure function of `input`
#   * Wall-clock / external state, when needed by a future rule, MUST
#     arrive via `input.now_ns` / `input.resource.state.*` — never via
#     a built-in side effect
#
# Replay-safety: the same `input` always produces the same verdict;
# there is no hidden state. Downstream audit can re-evaluate any past
# decision by replaying the recorded input.
#
# Backward-compatibility contract:
#   * `input.command` and `input.resource.state` are OPTIONAL. When
#     absent (pre-wiring), rules fall through to role-based allow so
#     existing callers are not silently broken; the aggregate still
#     catches every violation end-of-pipeline. When PRESENT, the
#     tighter state-aware checks fire and reject at the policy
#     boundary — defense-in-depth without drift.
# ──────────────────────────────────────────────────────────────────────

import rego.v1

default allow := false

# ── Common structural guard ───────────────────────────────────────────

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "obligation"
}

# ── whyce.economic.ledger.obligation.create ───────────────────────────
# Operator or admin may open an obligation. When the command payload is
# supplied via input.command, amount/counterparty/type invariants are
# mirrored from ObligationAggregate.Create.

allow if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.subject.role in {"operator", "admin"}
    valid_resource
    create_preconditions
}

create_preconditions if { not input.command }
create_preconditions if {
    input.command.amount > 0
    input.command.counterparty_id != ""
    input.command.counterparty_id != "00000000-0000-0000-0000-000000000000"
    input.command.type in {"Payable", "Receivable"}
}

# ── whyce.economic.ledger.obligation.fulfil ───────────────────────────
# Operator or admin may fulfil a Pending obligation. Terminal states
# (Fulfilled / Cancelled) are rejected — mirrors the aggregate's
# ObligationNotPending / AlreadyFulfilled / CannotFulfilCancelled guards.

allow if {
    input.policy_id == "whyce.economic.ledger.obligation.fulfil"
    input.subject.role in {"operator", "admin"}
    valid_resource
    fulfilment_state_ok
}

fulfilment_state_ok if { not input.resource.state }
fulfilment_state_ok if { input.resource.state.status == "Pending" }

# ── whyce.economic.ledger.obligation.cancel ───────────────────────────
# Admin-only: cancellation is an irreversible policy act whose blast
# radius is the entire counterparty relationship. Pending-only — mirrors
# the aggregate's ObligationNotPending / AlreadyCancelled /
# CannotCancelFulfilled guards.

allow if {
    input.policy_id == "whyce.economic.ledger.obligation.cancel"
    input.subject.role == "admin"
    valid_resource
    cancellation_state_ok
}

cancellation_state_ok if { not input.resource.state }
cancellation_state_ok if { input.resource.state.status == "Pending" }

# ── Structural deny rules ─────────────────────────────────────────────
# Missing fundamentals → immediate deny, independent of any allow path.
# These guard against a malformed or partially-populated input shape
# reaching the match arms above.

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }

# ── State-aware deny reasons ──────────────────────────────────────────
# Fires when input.resource.state / input.command IS supplied and the
# input rules out the transition. Kept as a separate partial set so the
# audit trail records WHY a request was refused — the aggregate's error
# message is mirrored at the policy boundary.

deny_reason contains "obligation_already_terminal" if {
    input.policy_id == "whyce.economic.ledger.obligation.fulfil"
    input.resource.state.status in {"Fulfilled", "Cancelled"}
}

deny_reason contains "obligation_already_terminal" if {
    input.policy_id == "whyce.economic.ledger.obligation.cancel"
    input.resource.state.status in {"Fulfilled", "Cancelled"}
}

deny_reason contains "obligation_amount_non_positive" if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.command.amount <= 0
}

deny_reason contains "obligation_counterparty_missing" if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.command.counterparty_id == ""
}

deny_reason contains "obligation_counterparty_missing" if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.command.counterparty_id == "00000000-0000-0000-0000-000000000000"
}

deny_reason contains "obligation_type_unknown" if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.command.type != ""
    not input.command.type in {"Payable", "Receivable"}
}
