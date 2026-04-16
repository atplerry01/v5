package whyce.policy.economic.routing.execution

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.routing.execution.start — Operator (system-initiated).
allow if {
    input.policy_id == "whyce.economic.routing.execution.start"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.routing.execution.complete — Operator (system-initiated).
allow if {
    input.policy_id == "whyce.economic.routing.execution.complete"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.routing.execution.fail — Operator (system-initiated terminal failure).
allow if {
    input.policy_id == "whyce.economic.routing.execution.fail"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.routing.execution.abort — Admin OR Operator
# (operator allowed so the runtime can auto-abort on upstream enforcement).
allow if {
    input.policy_id == "whyce.economic.routing.execution.abort"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.routing.execution.abort"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.routing.execution.read — Admin OR Operator.
allow if {
    input.policy_id == "whyce.economic.routing.execution.read"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.routing.execution.read"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "routing"
    input.resource.domain == "execution"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
