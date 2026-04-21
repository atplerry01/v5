package whyce.policy.business.offering.product

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.catalog-core.product.create
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.product.update
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.product.activate
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.product.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.product.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every product allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/offering/catalog-core/product still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "product"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
