package whyce.policy.business.service.service_definition

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.service.service-core.service-definition.create
# Authoring party (owner) OR Operator may create.
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-definition.update
# Only the owner of the service-definition or an operator may update.
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-definition.activate
# Owner or operator.
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-core.service-definition.archive
# Admin only — terminal lifecycle state.
allow if {
    input.policy_id == "whyce.business.service.service-core.service-definition.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is (classification, context, domain); the four-level
# physical path business/service/service-core/service-definition still projects onto
# this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "service"
    input.resource.domain == "service-definition"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
