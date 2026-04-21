package whyce.policy.business.agreement.clause

import rego.v1

default allow := false

# whyce.business.agreement.change-control.clause.create
allow if {
    input.policy_id == "whyce.business.agreement.change-control.clause.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.clause.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.change-control.clause.activate
allow if {
    input.policy_id == "whyce.business.agreement.change-control.clause.activate"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.change-control.clause.activate"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.change-control.clause.supersede
allow if {
    input.policy_id == "whyce.business.agreement.change-control.clause.supersede"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "clause"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
