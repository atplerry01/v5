package whyce.policy.content.document.core_object.file

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.core_object.file.register
allow if {
    input.policy_id == "whyce.content.document.core_object.file.register"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.file.verify_integrity
allow if {
    input.policy_id == "whyce.content.document.core_object.file.verify_integrity"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.file.supersede
allow if {
    input.policy_id == "whyce.content.document.core_object.file.supersede"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.file.archive
allow if {
    input.policy_id == "whyce.content.document.core_object.file.archive"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
