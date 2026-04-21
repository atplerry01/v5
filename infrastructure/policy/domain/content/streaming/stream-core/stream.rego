package whyce.policy.content.streaming.stream_core.stream

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.pause"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.resume"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.end"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.stream.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
