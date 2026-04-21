package whyce.policy.business.customer.customer

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.customer.identity-and-profile.customer.create
# Owner or operator may create a customer.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.customer.rename
# Owner of the customer or operator may rename.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.rename"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.rename"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.customer.reclassify
# Admin only — changing customer type has compliance implications.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.reclassify"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.customer.identity-and-profile.customer.activate
# Owner or operator.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.customer.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.customer.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every customer allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/customer/identity-and-profile/customer still projects onto
# the three-tuple (business, customer, customer).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "customer"
    input.resource.domain == "customer"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
