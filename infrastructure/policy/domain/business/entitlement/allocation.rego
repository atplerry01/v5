package whyce.policy.business.entitlement.allocation

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.usage-control.allocation.create
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.allocation.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.allocation.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.allocation.allocate
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.allocation.allocate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.allocation.allocate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.allocation.release
# Admin only (release reclaims capacity — operational safety action).
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.allocation.release"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every allocation allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/entitlement/usage-control/allocation still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "allocation"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
