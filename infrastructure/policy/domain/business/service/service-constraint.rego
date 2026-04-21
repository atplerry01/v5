package whyce.policy.business.service.service_constraint

import rego.v1

default allow := false

# whyce.business.service.service-constraint.service-constraint.create
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-constraint.update
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-constraint.activate
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-constraint.archive
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-constraint.archive"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "service"
    input.resource.domain == "service-constraint"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
