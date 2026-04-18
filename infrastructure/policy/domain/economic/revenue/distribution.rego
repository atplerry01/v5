package whyce.policy.economic.revenue.distribution

import rego.v1

# Phase 3.5 T3.5.3 — per-action authorization for the canonical economic
# pipeline (Revenue → Distribution → Payout → Ledger). Every binding in
# RevenuePolicyModule for the distribution domain is matched on
# input.policy_id below; every other policy_id (or a request without one)
# fails closed via the default deny.
#
# Bindings covered:
#   whyce.economic.revenue.distribution.create
#   whyce.economic.revenue.distribution.confirm
#   whyce.economic.revenue.distribution.mark_paid
#   whyce.economic.revenue.distribution.mark_failed

default allow := false

# Create — operator or admin may originate a distribution.
allow if {
    input.policy_id == "whyce.economic.revenue.distribution.create"
    input.subject.role in {"operator", "admin", "revenue-admin"}
    valid_resource
}

# Confirm — only revenue-admin or admin may move Created → Confirmed,
# because Confirmed is the gate that triggers payout.
allow if {
    input.policy_id == "whyce.economic.revenue.distribution.confirm"
    input.subject.role in {"revenue-admin", "admin"}
    valid_resource
}

# MarkPaid — system-originated only (payout workflow's MarkDistributionPaidStep).
# Hand-rolled MarkPaid against an unsigned distribution is rejected.
allow if {
    input.policy_id == "whyce.economic.revenue.distribution.mark_paid"
    input.is_system == true
    valid_resource
}

# MarkFailed — system or revenue-admin may transition to Failed for recovery.
allow if {
    input.policy_id == "whyce.economic.revenue.distribution.mark_failed"
    input.is_system == true
    valid_resource
}

allow if {
    input.policy_id == "whyce.economic.revenue.distribution.mark_failed"
    input.subject.role in {"revenue-admin", "admin"}
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "revenue"
    input.resource.domain == "distribution"
}

deny if { not input.policy_id }
deny if { not input.resource.classification }
