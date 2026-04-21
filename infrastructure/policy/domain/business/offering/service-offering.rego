package whyce.policy.business.offering.service_offering

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.offering.catalog-core.service-offering.create
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.service-offering.update
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.service-offering.activate
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.offering.catalog-core.service-offering.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.offering.catalog-core.service-offering.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every service-offering allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/offering/catalog-core/service-offering still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "offering"
    input.resource.domain == "service-offering"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
