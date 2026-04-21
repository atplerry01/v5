package whyce.policy.content.document.intake.upload

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.intake.upload.request
allow if {
    input.policy_id == "whyce.content.document.intake.upload.request"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.intake.upload.accept
allow if {
    input.policy_id == "whyce.content.document.intake.upload.accept"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.intake.upload.start_processing
allow if {
    input.policy_id == "whyce.content.document.intake.upload.start_processing"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.intake.upload.complete
allow if {
    input.policy_id == "whyce.content.document.intake.upload.complete"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.intake.upload.fail
allow if {
    input.policy_id == "whyce.content.document.intake.upload.fail"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.intake.upload.cancel
allow if {
    input.policy_id == "whyce.content.document.intake.upload.cancel"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
