package whyce.policy.business.offering.catalog

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.catalog-core.catalog.create
# Authoring party (owner) OR operator may create catalogs.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.catalog.add_member
# Only the owner of the catalog or an operator may add members.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.add_member"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.add_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.catalog.remove_member
# Only the owner of the catalog or an operator may remove members.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.remove_member"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.remove_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.catalog.publish
# Owner or operator.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.publish"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.publish"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.catalog.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.catalog.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every catalog allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/offering/catalog-core/catalog still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "catalog"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
