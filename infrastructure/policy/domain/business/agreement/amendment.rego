package whyce.policy.business.agreement.amendment

import rego.v1

default allow := false

# whyce.business.agreement.change-control.amendment.create
allow if {
    input.policy_id == "whyce.business.agreement.change-control.amendment.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.amendment.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.change-control.amendment.apply
allow if {
    input.policy_id == "whyce.business.agreement.change-control.amendment.apply"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.amendment.apply"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.change-control.amendment.revert
allow if {
    input.policy_id == "whyce.business.agreement.change-control.amendment.revert"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "amendment"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
