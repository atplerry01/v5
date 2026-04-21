package whyce.policy.business.entitlement.assignment

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.eligibility-and-grant.assignment.create
# Owner of the grant or operator may create an assignment.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.assignment.create"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.assignment.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.assignment.activate
# Owner or operator.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.assignment.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.assignment.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.assignment.revoke
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.assignment.revoke"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every assignment allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/entitlement/eligibility-and-grant/assignment still projects
# onto the three-tuple (business, entitlement, assignment).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "assignment"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
