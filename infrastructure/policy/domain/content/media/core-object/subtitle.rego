package whyce.policy.content.media.core_object.subtitle

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.core_object.subtitle.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.subtitle.update"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.subtitle.finalize"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.subtitle.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
