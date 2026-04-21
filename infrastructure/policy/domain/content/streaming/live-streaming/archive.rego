package whyce.policy.content.streaming.live_streaming.archive

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.live_streaming.archive.start"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.archive.complete"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.archive.fail"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.archive.finalize"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.live_streaming.archive.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
