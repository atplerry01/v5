package whyce.policy.economic.capital.allocation

import rego.v1

default allow := false

# whyce.economic.capital.allocation.create
# Owner of source account OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.allocation.create"
    input.subject.role == "owner"
    input.subject.is_owner_of_source_account == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.allocation.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.allocation.release
# Owner of source account (allocation must be Pending) OR Operator.
# The Pending-state check is enforced by the domain; policy gates by role+ownership.
allow if {
    input.policy_id == "whyce.economic.capital.allocation.release"
    input.subject.role == "owner"
    input.subject.is_owner_of_source_account == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.allocation.release"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.allocation.complete
# Operator only — completion is system-driven (target receipt confirmed).
allow if {
    input.policy_id == "whyce.economic.capital.allocation.complete"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.allocation.spv_declare
# Operator only — SPV mapping is system-managed.
allow if {
    input.policy_id == "whyce.economic.capital.allocation.spv_declare"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "allocation"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
