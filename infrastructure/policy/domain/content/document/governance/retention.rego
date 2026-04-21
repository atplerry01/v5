package whyce.policy.content.document.governance.retention

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.content.document.governance.retention.apply
allow if {
    input.policy_id == "whyce.content.document.governance.retention.apply"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.governance.retention.place_hold
allow if {
    input.policy_id == "whyce.content.document.governance.retention.place_hold"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.governance.retention.release
allow if {
    input.policy_id == "whyce.content.document.governance.retention.release"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.governance.retention.expire
allow if {
    input.policy_id == "whyce.content.document.governance.retention.expire"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.governance.retention.mark_eligible_for_destruction
allow if {
    input.policy_id == "whyce.content.document.governance.retention.mark_eligible_for_destruction"
    input.subject.role == "operator"
    valid_resource
}

# whyce.content.document.governance.retention.archive
allow if {
    input.policy_id == "whyce.content.document.governance.retention.archive"
    input.subject.role == "operator"
    valid_resource
}

valid_resource if {
    input.resource.classification in ["content", "system"]
}
