package whyce.policy.business.offering.configuration

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.commercial-shape.configuration.create
# Authoring party (owner) OR Operator (system-provisioning) may create.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.configuration.set_option
# Owner or operator may mutate options while the configuration is mutable.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.set_option"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.set_option"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.configuration.remove_option
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.remove_option"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.remove_option"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.configuration.activate
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.configuration.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.configuration.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is the three-tuple (classification, context, domain)
# even though the physical nesting is four-level
# (classification/context/domain-group/domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "configuration"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
