package whyce.policy.economic.capital.reserve

import rego.v1

default allow := false

# whyce.economic.capital.reserve.create
# Owner (must own the AccountId being reserved against) OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.reserve.create"
    input.subject.role == "owner"
    input.subject.is_owner_of_account == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.reserve.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.reserve.release
# Owner (of the reserve) OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.reserve.release"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.reserve.release"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.reserve.expire
# Operator only — time-driven; T1M workflow trigger.
allow if {
    input.policy_id == "whyce.economic.capital.reserve.expire"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "reserve"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
