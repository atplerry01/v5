package whyce.policy.business.service.service_option

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.service.service-core.service-option.create
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-option.update
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-option.activate
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-option.archive
allow if {
    input.policy_id == "whyce.business.service.service-core.service-option.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is (classification, context, domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "service"
    input.resource.domain == "service-option"
}

# Hard denies.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
