package whyce.policy.business.provider.provider_capability

import rego.v1

default allow := false

# whyce.business.provider.provider-core.provider-capability.create
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-core.provider-capability.update
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.update"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-core.provider-capability.activate
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-core.provider-capability.archive
allow if {
    input.policy_id == "whyce.business.provider.provider-core.provider-capability.archive"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "provider"
    input.resource.domain == "provider-capability"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
