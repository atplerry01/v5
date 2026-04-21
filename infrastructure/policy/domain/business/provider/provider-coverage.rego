package whyce.policy.business.provider.provider_coverage

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.provider.provider-scope.provider-coverage.create
# Authoring party (owner) OR operator may create coverage records.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-coverage.add_scope
# Only the owner or operator may add scopes.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.add_scope"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.add_scope"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-coverage.remove_scope
# Only the owner or operator may remove scopes.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.remove_scope"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.remove_scope"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-coverage.activate
# Owner or operator; aggregate also enforces "at least one scope" invariant.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-scope.provider-coverage.archive
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.provider.provider-scope.provider-coverage.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every provider-coverage allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/provider/provider-scope/provider-coverage still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "provider"
    input.resource.domain == "provider-coverage"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
