package whyce.policy.content.streaming.live_streaming.broadcast

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.schedule"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.start"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.pause"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.resume"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.end"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.broadcast.cancel"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
