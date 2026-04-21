package whyce.policy.business.agreement.renewal

import rego.v1

default allow := false

# whyce.business.agreement.change-control.renewal.create
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.change-control.renewal.renew
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.renew"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.renew"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.change-control.renewal.expire
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.expire"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.renewal.expire"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "renewal"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
