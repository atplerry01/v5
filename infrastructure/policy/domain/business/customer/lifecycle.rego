package whyce.policy.business.customer.lifecycle

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.customer.segmentation-and-lifecycle.lifecycle.start
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.start"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.start"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.lifecycle.change_stage
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.change_stage"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.change_stage"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.lifecycle.close
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.close"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.lifecycle.close"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every lifecycle allow path requires correct route.
# DomainRoute is (classification, context, domain) = (business, customer, lifecycle).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "customer"
    input.resource.domain == "lifecycle"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
