package whyce.policy.business.agreement.acceptance

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.commitment.acceptance.create
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.acceptance.accept
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.accept"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.accept"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.acceptance.reject
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.reject"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.acceptance.reject"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every acceptance allow path requires correct route.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "acceptance"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
