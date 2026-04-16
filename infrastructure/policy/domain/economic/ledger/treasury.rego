package whyce.policy.economic.ledger.treasury

import rego.v1

default allow := false

# whyce.economic.ledger.treasury.create
allow if {
    input.policy_id == "whyce.economic.ledger.treasury.create"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

# whyce.economic.ledger.treasury.allocate_funds
# Operator may allocate; admin may also allocate.
allow if {
    input.policy_id == "whyce.economic.ledger.treasury.allocate_funds"
    input.subject.role in {"operator", "admin"}
    valid_resource
}

# whyce.economic.ledger.treasury.release_funds
# Admin-only release to avoid unauthorised drawdowns.
allow if {
    input.policy_id == "whyce.economic.ledger.treasury.release_funds"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "treasury"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
