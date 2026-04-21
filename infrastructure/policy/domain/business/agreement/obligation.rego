package whyce.policy.business.agreement.obligation

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.commitment.obligation.create
allow if {
    input.policy_id == "whyce.business.agreement.commitment.obligation.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.obligation.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.obligation.fulfill
allow if {
    input.policy_id == "whyce.business.agreement.commitment.obligation.fulfill"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.obligation.fulfill"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.obligation.breach
# Admin only (operational/compliance action).
allow if {
    input.policy_id == "whyce.business.agreement.commitment.obligation.breach"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every obligation allow path requires correct route.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "obligation"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
