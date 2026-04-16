package whyce.policy.economic.subject.subject

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.subject.subject.register — Admin OR Operator (bridge authoring).
allow if {
    input.policy_id == "whyce.economic.subject.subject.register"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.subject.subject.register"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.subject.subject.read — Admin OR Operator.
allow if {
    input.policy_id == "whyce.economic.subject.subject.read"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.subject.subject.read"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "subject"
    input.resource.domain == "subject"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
