package whyce.policy.business.provider.provider_availability

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.provider.provider-scope.provider-availability.create
# Authoring party (owner) OR operator may create availability records.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-availability.update
# Owner of the availability record, or operator, may update the window.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-availability.activate
# Owner or operator; aggregate also enforces Draft → Active transition.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-availability.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-availability.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every provider-availability allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/provider/provider-scope/provider-availability still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "provider"
    input.resource.domain == "provider-availability"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
