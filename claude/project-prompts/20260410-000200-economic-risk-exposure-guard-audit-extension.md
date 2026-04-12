# PROMPT: economic-system/risk/exposure Guard & Audit Extension

## TITLE

Guard & Audit Extension — economic-system / risk / exposure

## CONTEXT

- **Classification**: economic-system
- **Context**: risk
- **Domain**: exposure

## OBJECTIVE

Add cross-domain rules (X1–X5), domain constraints (D51–D55), and violation codes (C36–C40) to the economic guard file for the exposure bounded context.

## CONSTRAINTS

- Rules appended to existing `claude/guards/domain-aligned/economic.guard.md`
- Follows established pattern from Transaction (T1–T5, D41–D45, C26–C30) and Revenue (R1–R5, D46–D50, C31–C35) sections
- Violation codes continue sequential numbering (C36–C40)
- Domain constraint IDs continue sequential numbering (D51–D55)

## EXECUTION STEPS

1. Append X-RULES section (X1–X5) for cross-domain exposure constraints
2. Append D-RULES section (D51–D55) for domain-level constraints
3. Append C-CONSTRAINTS table (C36–C40) for violation codes
4. Append canonical event flow for exposure lifecycle
5. Append check procedure and fail criteria

## OUTPUT FORMAT

Updated `economic.guard.md` with exposure rules appended after revenue section.

## VALIDATION CRITERIA

- Rule IDs are unique and sequential (X1–X5, D51–D55, C36–C40)
- No overlap with Transaction or Revenue rule IDs
- Check procedure covers all D-rules
- Fail criteria map to all C-constraints
- Severity levels assigned correctly (S0 for data integrity, S1 for flow)
