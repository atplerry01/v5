package whyce.policy.business.service.service_window

import rego.v1

default allow := false

# whyce.business.service.service-constraint.service-window.create
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-window.update
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-window.activate
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.service-window.archive
allow if {
    input.policy_id == "whyce.business.service.service-constraint.service-window.archive"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "service"
    input.resource.domain == "service-window"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
