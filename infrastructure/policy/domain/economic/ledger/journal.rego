package whyce.policy.economic.ledger.journal

import rego.v1

default allow := false

# whyce.economic.ledger.journal.post
# Operator or admin may post journal entries. Binding is anticipatory —
# `LedgerPolicyModule` does not yet stamp this id on PostJournalEntriesCommand.
# Adding the binding is a follow-up step; this package exists so PB-04 scope
# resolution does not fail when the binding lands.
allow if {
    input.policy_id == "whyce.economic.ledger.journal.post"
    input.subject.role in {"operator", "admin"}
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
