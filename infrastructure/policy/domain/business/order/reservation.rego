package whyce.policy.business.order.reservation

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.order.order-core.reservation.hold
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.hold"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.hold"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-core.reservation.confirm
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.confirm"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.confirm"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-core.reservation.release
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.release"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.release"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.order.order-core.reservation.expire — system/timer action.
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.expire"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-core.reservation.expire"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute is (classification, context, domain);
# the four-level physical path business/order/order-core/reservation still
# projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "order"
    input.resource.domain == "reservation"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
