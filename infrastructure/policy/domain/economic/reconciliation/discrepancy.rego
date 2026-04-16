package whyce.policy.economic.reconciliation.discrepancy

import rego.v1

default allow := false

# whyce.economic.reconciliation.discrepancy.detect — Operator (system-detected).
allow if {
    input.policy_id == "whyce.economic.reconciliation.discrepancy.detect"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.reconciliation.discrepancy.investigate — Admin OR Operator.
allow if {
    input.policy_id == "whyce.economic.reconciliation.discrepancy.investigate"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.reconciliation.discrepancy.investigate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.reconciliation.discrepancy.resolve — Admin OR Operator
# (operator allowed so the system reconciliation lifecycle worker can
# auto-close discrepancies it raised itself).
allow if {
    input.policy_id == "whyce.economic.reconciliation.discrepancy.resolve"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.reconciliation.discrepancy.resolve"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "reconciliation"
    input.resource.domain == "discrepancy"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
