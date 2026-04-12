package whyce.policy.operational.sandbox.kanban

import rego.v1

default allow := false

allow if {
    input.action == "kanban.create"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

allow if {
    input.action == "kanban.read"
    input.subject.role in {"viewer", "user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

allow if {
    input.action == "kanban.update"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

allow if {
    input.action == "kanban.move"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

allow if {
    input.action == "kanban.complete"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

allow if {
    input.action == "kanban.reorder"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "kanban"
}

deny if {
    not input.policy_id
}

deny if {
    not input.subject.role
}
