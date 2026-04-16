package whyce.policy.economic.ledger.obligation

import rego.v1

default allow := false

# whyce.economic.ledger.obligation.create
allow if {
    input.policy_id == "whyce.economic.ledger.obligation.create"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

# whyce.economic.ledger.obligation.fulfil
# Operator fulfils settlement-backed obligations.
allow if {
    input.policy_id == "whyce.economic.ledger.obligation.fulfil"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.ledger.obligation.fulfil"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.ledger.obligation.cancel
# Admin-only: cancel is irreversible once accepted.
allow if {
    input.policy_id == "whyce.economic.ledger.obligation.cancel"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "obligation"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
