package whyce.policy.content.document.core_object.bundle

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.core_object.bundle.create
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.bundle.rename
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.rename"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.bundle.add_member
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.add_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.bundle.remove_member
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.remove_member"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.bundle.finalize
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.finalize"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.bundle.archive
allow if {
    input.policy_id == "whyce.content.document.core_object.bundle.archive"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
