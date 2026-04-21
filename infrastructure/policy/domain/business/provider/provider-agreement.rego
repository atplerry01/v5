package whyce.policy.business.provider.provider_agreement

import rego.v1

default allow := false

# whyce.business.provider.provider-governance.provider-agreement.create
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-governance.provider-agreement.activate
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.provider.provider-governance.provider-agreement.suspend (admin only)
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.suspend"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.provider.provider-governance.provider-agreement.terminate (admin only — terminal)
allow if {
    input.policy_id == "whyce.business.provider.provider-governance.provider-agreement.terminate"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "provider"
    input.resource.domain == "provider-agreement"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
