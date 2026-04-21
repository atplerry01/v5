package whyce.policy.content.streaming.playback_consumption.session

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.open"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.resume"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.close"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.fail"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.playback_consumption.session.expire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
