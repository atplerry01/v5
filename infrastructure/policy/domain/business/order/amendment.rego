package whyce.policy.business.order.amendment

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.order.order-change.amendment.request
# Owner (the ordering customer) or operator may request an amendment.
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.request"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.request"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.amendment.accept
# Operator approves on behalf of the seller/provider.
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.accept"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.amendment.apply
# Operator applies the accepted amendment against the order.
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.apply"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.amendment.reject
# Operator rejects a pending amendment.
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.reject"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.amendment.cancel
# Owner cancels their own pending amendment; operator may always cancel.
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.cancel"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-change.amendment.cancel"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — DomainRoute is (classification, context, domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "order"
    input.resource.domain == "amendment"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
