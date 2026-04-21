package whyce.policy.content.document.core_object.record

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.core_object.record.create
allow if {
    input.policy_id == "whyce.content.document.core_object.record.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.record.lock
allow if {
    input.policy_id == "whyce.content.document.core_object.record.lock"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.record.unlock
allow if {
    input.policy_id == "whyce.content.document.core_object.record.unlock"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.record.close
allow if {
    input.policy_id == "whyce.content.document.core_object.record.close"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.record.archive
allow if {
    input.policy_id == "whyce.content.document.core_object.record.archive"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
