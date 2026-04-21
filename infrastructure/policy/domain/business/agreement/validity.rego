package whyce.policy.business.agreement.validity

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.commitment.validity.create
allow if {
    input.policy_id == "whyce.business.agreement.commitment.validity.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.validity.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.validity.expire
# Operator may expire (time-driven); admin may force-expire.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.validity.expire"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.validity.expire"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.commitment.validity.invalidate
# Admin only — compliance/legal action.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.validity.invalidate"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every validity allow path requires correct route.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "validity"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
