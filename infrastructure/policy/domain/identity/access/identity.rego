package whyce.policy.identity.access.identity

import rego.v1

default allow := false

allow if {
    input.action == "identity.authenticate"
    input.resource.classification == "identity"
    input.resource.context == "access"
    input.resource.domain == "identity"
}

allow if {
    input.action == "identity.read"
    input.subject.role in {"viewer", "operator", "admin"}
    input.resource.classification == "identity"
    input.resource.context == "access"
    input.resource.domain == "identity"
}

allow if {
    input.action == "identity.manage"
    input.subject.role in {"operator", "admin"}
    input.resource.classification == "identity"
    input.resource.context == "access"
    input.resource.domain == "identity"
}

deny if {
    not input.policy_id
}

deny if {
    not input.subject.role
}
