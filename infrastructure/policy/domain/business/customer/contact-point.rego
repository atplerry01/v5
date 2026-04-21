package whyce.policy.business.customer.contact_point

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.customer.segmentation-and-lifecycle.contact-point.create
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.contact-point.update
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.contact-point.activate
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.contact-point.set_preferred
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.set_preferred"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.set_preferred"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.segmentation-and-lifecycle.contact-point.archive
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.archive"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.segmentation-and-lifecycle.contact-point.archive"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every contact-point allow path requires correct route.
# DomainRoute is (classification, context, domain) = (business, customer, contact-point).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "customer"
    input.resource.domain == "contact-point"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
