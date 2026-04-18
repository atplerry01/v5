package whyce.policy.economic.transaction.limit

import rego.v1

# Phase 4 T4.1 — authorization for the per-account limit gate. The
# transaction control plane (CheckLimitStep) dispatches CheckLimitCommand
# via DispatchSystemAsync from inside the T1M lifecycle, so input.is_system
# is the primary acceptance route; operator/admin paths cover ad-hoc
# probing through the API surface.
#
# Bindings covered (TransactionPolicyModule):
#   whyce.economic.transaction.limit.define
#   whyce.economic.transaction.limit.check

default allow := false

# Define — only operator/admin may install a per-account limit.
allow if {
    input.policy_id == "whyce.economic.transaction.limit.define"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

# Check — system path: every transaction lifecycle execution dispatches
# CheckLimitCommand via DispatchSystemAsync. Hard-block on breach happens
# in the aggregate (LimitErrors.LimitExceeded), not in policy.
allow if {
    input.policy_id == "whyce.economic.transaction.limit.check"
    input.is_system == true
    valid_resource
}

allow if {
    input.policy_id == "whyce.economic.transaction.limit.check"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "transaction"
    input.resource.domain == "limit"
}

deny if { not input.policy_id }
deny if { not input.resource.classification }
