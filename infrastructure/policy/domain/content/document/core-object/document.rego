package whyce.policy.content.document.core_object.document

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.core_object.document.create
allow if {
    input.policy_id == "whyce.content.document.core_object.document.create"
    input.subject.role == "owner"
    input.subject.structural_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.update_metadata
allow if {
    input.policy_id == "whyce.content.document.core_object.document.update_metadata"
    input.subject.role == "owner"
    input.subject.document_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.update_metadata"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.attach_version
allow if {
    input.policy_id == "whyce.content.document.core_object.document.attach_version"
    input.subject.role == "owner"
    input.subject.document_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.attach_version"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.activate
allow if {
    input.policy_id == "whyce.content.document.core_object.document.activate"
    input.subject.role == "owner"
    input.subject.document_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.archive
allow if {
    input.policy_id == "whyce.content.document.core_object.document.archive"
    input.subject.role == "owner"
    input.subject.document_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.archive"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.restore
allow if {
    input.policy_id == "whyce.content.document.core_object.document.restore"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.core_object.document.supersede
allow if {
    input.policy_id == "whyce.content.document.core_object.document.supersede"
    input.subject.role == "owner"
    input.subject.document_owner_match == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.content.document.core_object.document.supersede"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
