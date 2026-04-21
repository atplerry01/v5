package whyce.policy.content.document.lifecycle_change.processing

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.lifecycle_change.processing.request
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.processing.request"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.processing.start
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.processing.start"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.processing.complete
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.processing.complete"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.processing.fail
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.processing.fail"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.lifecycle_change.processing.cancel
allow if {
    input.policy_id == "whyce.content.document.lifecycle_change.processing.cancel"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
