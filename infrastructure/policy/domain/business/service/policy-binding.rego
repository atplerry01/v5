package whyce.policy.business.service.policy_binding

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.service.service-constraint.policy-binding.create
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.policy-binding.bind
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.bind"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.bind"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.service.service-constraint.policy-binding.unbind
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.unbind"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.service.service-constraint.policy-binding.archive
allow if {
    input.policy_id == "whyce.business.service.service-constraint.policy-binding.archive"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — DomainRoute (classification, context, domain).
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "service"
    input.resource.domain == "policy-binding"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
