package whyce.policy.business.offering.plan

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.commercial-shape.plan.draft
# Authoring party (owner) OR Operator (system-provisioning) may draft.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.draft"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.draft"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.plan.activate
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.commercial-shape.plan.deprecate
# Admin-only action — lifecycle management.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.deprecate"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.offering.commercial-shape.plan.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.commercial-shape.plan.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is the three-tuple (classification, context, domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "plan"
}

# Hard denies.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
