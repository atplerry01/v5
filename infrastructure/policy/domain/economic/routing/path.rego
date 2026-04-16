package whyce.policy.economic.routing.path

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.routing.path.define — Admin OR Operator (new path authoring).
allow if {
    input.policy_id == "whyce.economic.routing.path.define"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.routing.path.define"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.routing.path.activate — Admin only (production control).
allow if {
    input.policy_id == "whyce.economic.routing.path.activate"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.routing.path.disable — Admin only (terminal, irreversible).
allow if {
    input.policy_id == "whyce.economic.routing.path.disable"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.routing.path.read — Admin OR Operator.
allow if {
    input.policy_id == "whyce.economic.routing.path.read"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.routing.path.read"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "routing"
    input.resource.domain == "path"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
