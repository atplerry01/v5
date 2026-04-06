package whyce.policy.operational.sandbox.todo

import rego.v1

default allow := false

allow if {
    input.action == "todo.create"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "todo"
}

allow if {
    input.action == "todo.read"
    input.subject.role in {"viewer", "user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "todo"
}

allow if {
    input.action == "todo.update"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "todo"
}

allow if {
    input.action == "todo.complete"
    input.subject.role in {"user", "operator", "admin"}
    input.resource.classification == "operational"
    input.resource.context == "sandbox"
    input.resource.domain == "todo"
}

deny if {
    not input.policy_id
}

deny if {
    not input.subject.role
}
