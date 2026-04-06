package whyce.policy.operational.global.incident

import rego.v1

default allow := false

allow if {
    input.action == "incident.report"
    input.subject.role in {"operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "global"
    input.resource.domain == "incident"
}

allow if {
    input.action == "incident.read"
    input.subject.role in {"viewer", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "global"
    input.resource.domain == "incident"
}

allow if {
    input.action == "incident.resolve"
    input.subject.role == "admin"
    input.resource.classification == "operational"
    input.resource.context == "global"
    input.resource.domain == "incident"
}

deny if {
    not input.policy_id
}

deny if {
    not input.subject.role
}
