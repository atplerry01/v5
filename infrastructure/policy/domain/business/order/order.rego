package whyce.policy.business.order.order

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.order.order-core.order.create
allow if {
    input.policy_id == "whyce.business.order.order-core.order.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.order.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-core.order.confirm
allow if {
    input.policy_id == "whyce.business.order.order-core.order.confirm"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.order.confirm"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-core.order.complete
allow if {
    input.policy_id == "whyce.business.order.order-core.order.complete"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.order.complete"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.order.order-core.order.cancel
allow if {
    input.policy_id == "whyce.business.order.order-core.order.cancel"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.order.cancel"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is (classification, context, domain);
# the four-level physical path business/order/order-core/order still
# projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "order"
    input.resource.domain == "order"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
