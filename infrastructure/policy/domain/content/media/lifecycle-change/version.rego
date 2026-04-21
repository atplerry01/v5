package whyce.policy.content.media.lifecycle_change.version

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.lifecycle_change.version.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.lifecycle_change.version.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.lifecycle_change.version.supersede"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.lifecycle_change.version.withdraw"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
