package whyce.policy.content.media.core_object.transcript

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.core_object.transcript.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.transcript.update"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.transcript.finalize"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.transcript.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
