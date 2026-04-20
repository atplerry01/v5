# review (SCAFFOLD — pending implementation)

## Purpose

The `review` leaf owns the **pre-publication review gate** for a document — a structured lifecycle for submitting a document for review, collecting reviewer decisions, and producing a go/no-go verdict that may feed publication.

## Owns

- Review identity, target ref (DocumentRef), reviewer set, per-reviewer decisions, status (Requested / InReview / Approved / Rejected / Withdrawn).
- Request / assign / record-decision / approve / reject / withdraw transitions.

## Does not own

- Approval-workflow orchestration across multiple documents — orchestration layer.
- Identity of reviewers — resolved through identity context.
- The publication event itself — owned by `lifecycle-change/publication`.

## Status

SCAFFOLD only in P2.6.CS.3.
