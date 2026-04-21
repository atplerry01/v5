package whyce.policy.content.streaming.stream_core.channel

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.stream_core.channel.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.channel.rename"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.channel.enable"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.channel.disable"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.stream_core.channel.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
