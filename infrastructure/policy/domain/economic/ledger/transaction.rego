package whyce.policy.economic.ledger.transaction

import rego.v1

default allow := false

allow if {
    input.action == "transaction.submit"
    input.subject.role in {"operator", "admin"}
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "transaction"
}

allow if {
    input.action == "transaction.read"
    input.subject.role in {"viewer", "operator", "admin"}
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "transaction"
}

deny if {
    not input.policy_id
}

deny if {
    not input.subject.role
}
