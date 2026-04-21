package whyce.policy.business.entitlement.limit

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.usage-control.limit.create
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.limit.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.limit.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.limit.enforce
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.limit.enforce"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.limit.enforce"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.limit.breach
# Admin only (breach is a compliance/audit-significant event).
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.limit.breach"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every limit allow path requires correct route.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "limit"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
