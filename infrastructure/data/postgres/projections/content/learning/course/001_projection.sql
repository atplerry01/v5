CREATE SCHEMA IF NOT EXISTS projection_content_learning_course;

CREATE TABLE projection_content_learning_course.course_read_model (
    aggregate_id        UUID        PRIMARY KEY,
    aggregate_type      TEXT        NOT NULL,
    current_version     INT         NOT NULL DEFAULT 0,
    state               JSONB       NOT NULL DEFAULT '{}',
    last_event_id       UUID,
    last_event_type     TEXT,
    correlation_id      UUID,
    idempotency_key     TEXT        UNIQUE,
    projected_at        TIMESTAMP   NOT NULL DEFAULT NOW(),
    created_at          TIMESTAMP   NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_clc_aggregate_type
    ON projection_content_learning_course.course_read_model (aggregate_type);

CREATE INDEX idx_clc_correlation_id
    ON projection_content_learning_course.course_read_model (correlation_id);

CREATE INDEX idx_clc_projected_at
    ON projection_content_learning_course.course_read_model (projected_at);

CREATE INDEX idx_clc_owner_ref
    ON projection_content_learning_course.course_read_model ((state->>'OwnerRef'));

CREATE INDEX idx_clc_status
    ON projection_content_learning_course.course_read_model ((state->>'Status'));
