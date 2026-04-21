package whyce.policy.business.customer.account

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.customer.identity-and-profile.account.create
# Owner or operator may create accounts for a customer.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.account.rename
# Owner of the account or operator may rename.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.rename"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.rename"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.account.activate
# Owner or operator — engine enforces state-machine invariants.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.customer.identity-and-profile.account.suspend
# Admin only (operational safety action).
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.suspend"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.customer.identity-and-profile.account.close
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.customer.identity-and-profile.account.close"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every account allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/customer/identity-and-profile/account still projects onto
# the three-tuple (business, customer, account).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "customer"
    input.resource.domain == "account"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
