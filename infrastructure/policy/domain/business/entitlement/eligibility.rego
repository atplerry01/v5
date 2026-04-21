package whyce.policy.business.entitlement.eligibility

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.eligibility-and-grant.eligibility.create
# Owner or operator may create an eligibility record.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.eligibility.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.eligibility.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.eligibility.mark_eligible
# Operator only — evaluation is a system-driven transition.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.eligibility.mark_eligible"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.eligibility-and-grant.eligibility.mark_ineligible
# Operator only — evaluation is a system-driven transition.
allow if {
    input.policy_id == "whyce.business.entitlement.eligibility-and-grant.eligibility.mark_ineligible"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every eligibility allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/entitlement/eligibility-and-grant/eligibility still projects
# onto the three-tuple (business, entitlement, eligibility).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "eligibility"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
