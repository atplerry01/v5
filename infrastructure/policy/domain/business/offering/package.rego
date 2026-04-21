package whyce.policy.business.offering.package

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.commercial-shape.package.create
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.package.add_member
# Owner or operator may add members while the package is mutable.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.add_member"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.add_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.package.remove_member
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.remove_member"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.remove_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.package.activate
# Activation requires at least one member — the aggregate enforces that
# invariant; this rule just gates on subject/role.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.package.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.package.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is the three-tuple (classification, context, domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "package"
}

# Hard denies.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
