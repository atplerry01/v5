package whyce.policy.business.agreement.approval

import rego.v1

default allow := false

# whyce.business.agreement.change-control.approval.create
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.change-control.approval.approve
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.approve"
    input.subject.role == "approver"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.approve"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.change-control.approval.reject
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.reject"
    input.subject.role == "approver"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.approval.reject"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "approval"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
