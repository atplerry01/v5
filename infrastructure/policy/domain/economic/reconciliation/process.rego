package whyce.policy.economic.reconciliation.process

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.reconciliation.process.trigger — Operator-initiated.
allow if {
    input.policy_id == "whyce.economic.reconciliation.process.trigger"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.reconciliation.process.matched
allow if {
    input.policy_id == "whyce.economic.reconciliation.process.matched"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.reconciliation.process.mismatched
allow if {
    input.policy_id == "whyce.economic.reconciliation.process.mismatched"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.reconciliation.process.resolve — Admin OR Operator.
allow if {
    input.policy_id == "whyce.economic.reconciliation.process.resolve"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.reconciliation.process.resolve"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "reconciliation"
    input.resource.domain == "process"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
