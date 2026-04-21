package whyce.policy.business.order.cancellation

import rego.v1

default allow := false

# whyce.business.order.order-change.cancellation.request
# Owner (the ordering customer) or operator may request cancellation.
allow if {
    input.policy_id == "whyce.business.order.order-change.cancellation.request"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-change.cancellation.request"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.cancellation.confirm
# Operator confirms cancellation on behalf of the seller/provider.
allow if {
    input.policy_id == "whyce.business.order.order-change.cancellation.confirm"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.cancellation.reject
# Operator rejects a cancellation request.
allow if {
    input.policy_id == "whyce.business.order.order-change.cancellation.reject"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "order"
    input.resource.domain == "cancellation"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
