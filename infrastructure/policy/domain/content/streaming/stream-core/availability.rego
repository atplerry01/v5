package whyce.policy.content.streaming.stream_core.availability

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.stream_core.availability.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.availability.enable"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.availability.disable"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.availability.update_window"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.availability.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
