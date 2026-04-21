package whyce.policy.business.entitlement.usage_right

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.entitlement.usage-control.usage-right.create
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.usage-right.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.usage-right.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.usage-right.use
# Owner/operator may consume units; engine enforces Remaining >= UnitsUsed.
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.usage-right.use"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.usage-right.use"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.entitlement.usage-control.usage-right.consume
# Admin only (terminal state — closes the usage right).
allow if {
    input.policy_id == "whyce.business.entitlement.usage-control.usage-right.consume"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every usage-right allow path requires correct route.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "entitlement"
    input.resource.domain == "usage-right"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
