package whyce.policy.content.document.descriptor.metadata

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.descriptor.metadata.create
allow if {
    input.policy_id == "whyce.content.document.descriptor.metadata.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.descriptor.metadata.add_entry
allow if {
    input.policy_id == "whyce.content.document.descriptor.metadata.add_entry"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.descriptor.metadata.update_entry
allow if {
    input.policy_id == "whyce.content.document.descriptor.metadata.update_entry"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.descriptor.metadata.remove_entry
allow if {
    input.policy_id == "whyce.content.document.descriptor.metadata.remove_entry"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.descriptor.metadata.finalize
allow if {
    input.policy_id == "whyce.content.document.descriptor.metadata.finalize"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
