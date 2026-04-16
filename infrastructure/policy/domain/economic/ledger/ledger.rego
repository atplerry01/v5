package whyce.policy.economic.ledger.ledger

import rego.v1

default allow := false

# whyce.economic.ledger.ledger.open
# Operator or admin may open a new ledger.
allow if {
    input.policy_id == "whyce.economic.ledger.ledger.open"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "ledger"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
