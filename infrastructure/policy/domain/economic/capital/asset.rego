package whyce.policy.economic.capital.asset

import rego.v1

default allow := false

# whyce.economic.capital.asset.create
# Owner (self-create) OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.asset.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.asset.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.asset.revalue
# Operator only — revaluation requires a valid valuation source attestation.
allow if {
    input.policy_id == "whyce.economic.capital.asset.revalue"
    input.subject.role == "operator"
    input.subject.valuation_source_attested == true
    valid_resource
}

# whyce.economic.capital.asset.dispose
# Owner of asset OR Admin.
allow if {
    input.policy_id == "whyce.economic.capital.asset.dispose"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.asset.dispose"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "asset"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
