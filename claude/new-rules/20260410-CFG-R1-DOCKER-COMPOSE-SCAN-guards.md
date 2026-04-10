CLASSIFICATION: guards
SOURCE: deep-sweep-20260410.audit.output.md (subagent: tests/infra/kafka/platform)
SEVERITY: S0

DESCRIPTION:
config-safety.guard CFG-R1 currently scans `appsettings*.json` and `.env*` files
for plaintext credentials but does not enforce against `docker-compose*.yml`.
infrastructure/deployment/multi-instance.compose.yml contains literal
`Password=whyce`, `MinIO__SecretKey: "whycepassword"`, `MinIO__AccessKey: "whyce"`
in version control. This is a real exposure, not theoretical.

PROPOSED_RULE:
Extend CFG-R1 file glob to include `**/docker-compose*.yml`, `**/*.compose.yml`,
`**/compose.yaml`. Forbidden patterns: literal Password=, SecretKey:, AccessKey:,
APIKey:, Token: with non-`${...}` right-hand-sides. Required form:
`${VAR_NAME}` substitution, with the variable defined in a gitignored `.env.local`.

PROMOTION TARGET: claude/guards/config-safety.guard.md
