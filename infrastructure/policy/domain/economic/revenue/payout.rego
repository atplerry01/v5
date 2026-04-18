package whyce.policy.economic.revenue.payout

import rego.v1

# Phase 3.5 T3.5.3 — per-action authorization for the payout domain inside
# the canonical economic pipeline. Every binding in RevenuePolicyModule for
# payout is matched on input.policy_id; the Phase 3 pipeline dispatches all
# three via DispatchSystemAsync from inside the T1M workflow, so the
# system-origin path is the primary acceptance route.
#
# Bindings covered:
#   whyce.economic.revenue.payout.execute
#   whyce.economic.revenue.payout.mark_executed
#   whyce.economic.revenue.payout.mark_failed

default allow := false

# Execute — revenue-admin or admin may originate a payout via the API surface;
# the T1M payout workflow runs as system-origin (RequestPayoutStep dispatches
# ExecutePayoutCommand via DispatchSystemAsync) and is also accepted here.
allow if {
    input.policy_id == "whyce.economic.revenue.payout.execute"
    input.subject.role in {"revenue-admin", "admin"}
    valid_resource
}

allow if {
    input.policy_id == "whyce.economic.revenue.payout.execute"
    input.is_system == true
    valid_resource
}

# MarkExecuted — system-only. Issued by MarkPayoutExecutedStep after vault
# movements complete and conservation is verified; no human path.
allow if {
    input.policy_id == "whyce.economic.revenue.payout.mark_executed"
    input.is_system == true
    valid_resource
}

# MarkFailed — system path for workflow rollback; admin path for manual
# recovery / reconciliation.
allow if {
    input.policy_id == "whyce.economic.revenue.payout.mark_failed"
    input.is_system == true
    valid_resource
}

allow if {
    input.policy_id == "whyce.economic.revenue.payout.mark_failed"
    input.subject.role in {"revenue-admin", "admin"}
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "revenue"
    input.resource.domain == "payout"
}

deny if { not input.policy_id }
deny if { not input.resource.classification }
