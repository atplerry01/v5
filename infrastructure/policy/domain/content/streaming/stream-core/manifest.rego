package whyce.policy.content.streaming.stream_core.manifest

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.stream_core.manifest.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.manifest.update"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.manifest.publish"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.manifest.retire"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.manifest.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
