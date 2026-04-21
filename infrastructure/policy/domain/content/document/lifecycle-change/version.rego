package whyce.policy.content.document.lifecycle_change.version

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.lifecycle_change.version.create
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.version.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.version.activate
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.version.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.version.supersede
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.version.supersede"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.version.withdraw
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.version.withdraw"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
