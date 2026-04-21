package whyce.policy.content.document.core_object.template

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.core_object.template.create
allow if {
    input.policy_id == "whyce.content.document.core_object.template.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.template.update
allow if {
    input.policy_id == "whyce.content.document.core_object.template.update"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.template.activate
allow if {
    input.policy_id == "whyce.content.document.core_object.template.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.template.deprecate
allow if {
    input.policy_id == "whyce.content.document.core_object.template.deprecate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.template.archive
allow if {
    input.policy_id == "whyce.content.document.core_object.template.archive"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
