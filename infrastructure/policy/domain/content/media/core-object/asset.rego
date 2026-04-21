package whyce.policy.content.media.core_object.asset

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.core_object.asset.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.asset.rename"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.asset.reclassify"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.asset.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.asset.retire"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.core_object.asset.assign_kind"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
