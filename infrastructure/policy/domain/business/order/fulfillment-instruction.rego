package whyce.policy.business.order.fulfillment_instruction

import rego.v1

default allow := false

# whyce.business.order.order-change.fulfillment-instruction.create
allow if {
    input.policy_id == "whyce.business.order.order-change.fulfillment-instruction.create"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.order.order-change.fulfillment-instruction.create"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.order.order-change.fulfillment-instruction.issue
allow if {
    input.policy_id == "whyce.business.order.order-change.fulfillment-instruction.issue"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.fulfillment-instruction.complete
allow if {
    input.policy_id == "whyce.business.order.order-change.fulfillment-instruction.complete"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.order.order-change.fulfillment-instruction.revoke
allow if {
    input.policy_id == "whyce.business.order.order-change.fulfillment-instruction.revoke"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "order"
    input.resource.domain == "fulfillment-instruction"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
