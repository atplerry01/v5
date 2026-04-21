package whyce.policy.business.customer.profile

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.customer.identity-and-profile.profile.create
# Owner or operator may create a profile.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.profile.rename
# Owner of the profile or operator may rename.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.rename"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.rename"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.profile.set_descriptor
# Owner or operator may add/update descriptors.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.set_descriptor"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.set_descriptor"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.profile.remove_descriptor
# Owner or operator may remove descriptors.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.remove_descriptor"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.remove_descriptor"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.profile.activate
# Owner or operator.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.profile.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.profile.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every profile allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/customer/identity-and-profile/profile still projects onto
# the three-tuple (business, customer, profile).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "customer"
    input.resource.domain == "profile"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
