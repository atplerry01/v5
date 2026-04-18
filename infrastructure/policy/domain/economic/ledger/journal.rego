package whyce.policy.economic.ledger.journal

import rego.v1

# Phase 4.5 T4.5.2 / T4.5.3 — control-plane origin gate for the ledger.
# The structural bypass closure lives in PostJournalEntriesHandler, which
# rejects any command with context.IsSystem == false. This rego policy
# layers an authorization gate on top of that engine-level check:
# only system-origin dispatches are admitted by policy, AND the admin
# role gates the operator escape-hatch surface (which currently fails
# closed at the engine boundary regardless).
#
# Allowed dispatch paths:
#   * Transaction lifecycle PostToLedgerStep   → DispatchSystemAsync → IsSystem=true
#   * Revenue payout       PostLedgerJournalStep → DispatchSystemAsync → IsSystem=true
# Direct API calls via JournalController use DispatchAsync (IsSystem=false)
# and are rejected at the engine boundary even if policy admits them.

default allow := false

# whyce.economic.ledger.journal.post — system-origin path (transaction
# lifecycle + payout pipeline). Always admitted; the per-account control
# checks ran upstream in the originating workflow.
allow if {
    input.policy_id == "whyce.economic.ledger.journal.post"
    input.is_system == true
    valid_resource
}

# whyce.economic.ledger.journal.post — admin operator path. Reserved for
# the JournalController escape-hatch shell. Even when admitted by policy,
# the engine-level IsSystem gate in PostJournalEntriesHandler still
# rejects, because the controller uses DispatchAsync. To genuinely allow
# this path, the dispatcher and handler must be extended with an explicit
# operator-origin flag — a deliberate two-gate model.
allow if {
    input.policy_id == "whyce.economic.ledger.journal.post"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "journal"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
