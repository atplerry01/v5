package whyce.policy.business.entitlement.grant

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.eligibility-and-grant.grant.create
# Owner or operator may create a grant.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.grant.activate
# Owner of the grant or operator.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.grant.revoke
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.revoke"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.grant.expire
# Operator only — expiry is a system-driven transition.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.grant.expire"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every grant allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/entitlement/eligibility-and-grant/grant still projects onto
# the three-tuple (business, entitlement, grant).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "grant"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
