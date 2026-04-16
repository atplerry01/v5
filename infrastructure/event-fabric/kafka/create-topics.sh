#!/bin/bash
set -e

BOOTSTRAP_SERVER="${KAFKA_BOOTSTRAP_SERVERS:-kafka:9092}"
PARTITIONS=12
REPLICATION=1

echo "Waiting for Kafka to be ready..."
sleep 5

# Domain-aligned topics: whyce.{classification}.{context}.{domain}.{type}
# Each bounded context gets: commands, events, retry, deadletter (dual-topic + DLQ)

TOPICS=(
  # economic > ledger > transaction
  "whyce.economic.ledger.transaction.commands"
  "whyce.economic.ledger.transaction.events"
  "whyce.economic.ledger.transaction.retry"
  "whyce.economic.ledger.transaction.deadletter"

  # identity > access > identity
  "whyce.identity.access.identity.commands"
  "whyce.identity.access.identity.events"
  "whyce.identity.access.identity.retry"
  "whyce.identity.access.identity.deadletter"

  # operational > global > incident
  "whyce.operational.global.incident.commands"
  "whyce.operational.global.incident.events"
  "whyce.operational.global.incident.retry"
  "whyce.operational.global.incident.deadletter"

  # operational > sandbox > todo
  "whyce.operational.sandbox.todo.commands"
  "whyce.operational.sandbox.todo.events"
  "whyce.operational.sandbox.todo.retry"
  "whyce.operational.sandbox.todo.deadletter"

  # operational > sandbox > kanban
  "whyce.operational.sandbox.kanban.commands"
  "whyce.operational.sandbox.kanban.events"
  "whyce.operational.sandbox.kanban.retry"
  "whyce.operational.sandbox.kanban.deadletter"

  # constitutional > policy > decision (Phase 1 gate unblock S0)
  "whyce.constitutional.policy.decision.commands"
  "whyce.constitutional.policy.decision.events"
  "whyce.constitutional.policy.decision.retry"
  "whyce.constitutional.policy.decision.deadletter"

  # economic > revenue > revenue (Phase 2E)
  "whyce.economic.revenue.revenue.commands"
  "whyce.economic.revenue.revenue.events"
  "whyce.economic.revenue.revenue.retry"
  "whyce.economic.revenue.revenue.deadletter"

  # economic > revenue > distribution (Phase 2E)
  "whyce.economic.revenue.distribution.commands"
  "whyce.economic.revenue.distribution.events"
  "whyce.economic.revenue.distribution.retry"
  "whyce.economic.revenue.distribution.deadletter"

  # economic > revenue > payout (Phase 2E)
  "whyce.economic.revenue.payout.commands"
  "whyce.economic.revenue.payout.events"
  "whyce.economic.revenue.payout.retry"
  "whyce.economic.revenue.payout.deadletter"

  # economic > revenue > contract (Phase 2E — Option 3 reconciliation)
  "whyce.economic.revenue.contract.commands"
  "whyce.economic.revenue.contract.events"
  "whyce.economic.revenue.contract.retry"
  "whyce.economic.revenue.contract.deadletter"

  # economic > revenue > pricing (Phase 2E — Option 3 reconciliation)
  "whyce.economic.revenue.pricing.commands"
  "whyce.economic.revenue.pricing.events"
  "whyce.economic.revenue.pricing.retry"
  "whyce.economic.revenue.pricing.deadletter"

  # economic > vault > account (Phase 2E — ApplyRevenue, DebitSlice, CreditSlice)
  "whyce.economic.vault.account.commands"
  "whyce.economic.vault.account.events"
  "whyce.economic.vault.account.retry"
  "whyce.economic.vault.account.deadletter"

  # economic > transaction > expense (Phase 2 — RecordExpense)
  "whyce.economic.transaction.expense.commands"
  "whyce.economic.transaction.expense.events"
  "whyce.economic.transaction.expense.retry"
  "whyce.economic.transaction.expense.deadletter"

  # economic > ledger > journal (Phase 2 — PostJournalEntries)
  "whyce.economic.ledger.journal.commands"
  "whyce.economic.ledger.journal.events"
  "whyce.economic.ledger.journal.retry"
  "whyce.economic.ledger.journal.deadletter"

  # economic > ledger > ledger (Phase 2 — LedgerUpdated)
  "whyce.economic.ledger.ledger.commands"
  "whyce.economic.ledger.ledger.events"
  "whyce.economic.ledger.ledger.retry"
  "whyce.economic.ledger.ledger.deadletter"

  # economic > ledger > entry (Phase 2 — immutable debit/credit records)
  "whyce.economic.ledger.entry.commands"
  "whyce.economic.ledger.entry.events"
  "whyce.economic.ledger.entry.retry"
  "whyce.economic.ledger.entry.deadletter"

  # economic > ledger > obligation (Phase 2 — Create / Fulfil / Cancel)
  "whyce.economic.ledger.obligation.commands"
  "whyce.economic.ledger.obligation.events"
  "whyce.economic.ledger.obligation.retry"
  "whyce.economic.ledger.obligation.deadletter"

  # economic > ledger > treasury (Phase 2 — Create / AllocateFunds / ReleaseFunds)
  "whyce.economic.ledger.treasury.commands"
  "whyce.economic.ledger.treasury.events"
  "whyce.economic.ledger.treasury.retry"
  "whyce.economic.ledger.treasury.deadletter"

  # economic > transaction > settlement (Phase 2 — InitiateSettlement / CompleteSettlement / FailSettlement)
  "whyce.economic.transaction.settlement.commands"
  "whyce.economic.transaction.settlement.events"
  "whyce.economic.transaction.settlement.retry"
  "whyce.economic.transaction.settlement.deadletter"

  # economic > transaction > transaction (Phase 2 — Initiate / Commit / Fail orchestration envelope)
  "whyce.economic.transaction.transaction.commands"
  "whyce.economic.transaction.transaction.events"
  "whyce.economic.transaction.transaction.retry"
  "whyce.economic.transaction.transaction.deadletter"

  # economic > transaction > charge (Phase 2 — Calculate / Apply)
  "whyce.economic.transaction.charge.commands"
  "whyce.economic.transaction.charge.events"
  "whyce.economic.transaction.charge.retry"
  "whyce.economic.transaction.charge.deadletter"

  # economic > transaction > instruction (Phase 2 — Create / Execute / Cancel)
  "whyce.economic.transaction.instruction.commands"
  "whyce.economic.transaction.instruction.events"
  "whyce.economic.transaction.instruction.retry"
  "whyce.economic.transaction.instruction.deadletter"

  # economic > transaction > limit (Phase 2 — Define / Check)
  "whyce.economic.transaction.limit.commands"
  "whyce.economic.transaction.limit.events"
  "whyce.economic.transaction.limit.retry"
  "whyce.economic.transaction.limit.deadletter"

  # economic > transaction > wallet (Phase 2 — Create / RequestTransaction)
  "whyce.economic.transaction.wallet.commands"
  "whyce.economic.transaction.wallet.events"
  "whyce.economic.transaction.wallet.retry"
  "whyce.economic.transaction.wallet.deadletter"

  # economic > capital > account (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.account.commands"
  "whyce.economic.capital.account.events"
  "whyce.economic.capital.account.retry"
  "whyce.economic.capital.account.deadletter"

  # economic > capital > allocation (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.allocation.commands"
  "whyce.economic.capital.allocation.events"
  "whyce.economic.capital.allocation.retry"
  "whyce.economic.capital.allocation.deadletter"

  # economic > capital > asset (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.asset.commands"
  "whyce.economic.capital.asset.events"
  "whyce.economic.capital.asset.retry"
  "whyce.economic.capital.asset.deadletter"

  # economic > capital > binding (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.binding.commands"
  "whyce.economic.capital.binding.events"
  "whyce.economic.capital.binding.retry"
  "whyce.economic.capital.binding.deadletter"

  # economic > capital > pool (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.pool.commands"
  "whyce.economic.capital.pool.events"
  "whyce.economic.capital.pool.retry"
  "whyce.economic.capital.pool.deadletter"

  # economic > capital > reserve (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.reserve.commands"
  "whyce.economic.capital.reserve.events"
  "whyce.economic.capital.reserve.retry"
  "whyce.economic.capital.reserve.deadletter"

  # economic > capital > vault (Phase 2 capital wiring — Option C)
  "whyce.economic.capital.vault.commands"
  "whyce.economic.capital.vault.events"
  "whyce.economic.capital.vault.retry"
  "whyce.economic.capital.vault.deadletter"

  # economic > enforcement > rule
  "whyce.economic.enforcement.rule.commands"
  "whyce.economic.enforcement.rule.events"
  "whyce.economic.enforcement.rule.retry"
  "whyce.economic.enforcement.rule.deadletter"

  # economic > enforcement > violation
  "whyce.economic.enforcement.violation.commands"
  "whyce.economic.enforcement.violation.events"
  "whyce.economic.enforcement.violation.retry"
  "whyce.economic.enforcement.violation.deadletter"

  # economic > enforcement > escalation
  "whyce.economic.enforcement.escalation.commands"
  "whyce.economic.enforcement.escalation.events"
  "whyce.economic.enforcement.escalation.retry"
  "whyce.economic.enforcement.escalation.deadletter"

  # economic > enforcement > lock
  "whyce.economic.enforcement.lock.commands"
  "whyce.economic.enforcement.lock.events"
  "whyce.economic.enforcement.lock.retry"
  "whyce.economic.enforcement.lock.deadletter"

  # economic > enforcement > restriction
  "whyce.economic.enforcement.restriction.commands"
  "whyce.economic.enforcement.restriction.events"
  "whyce.economic.enforcement.restriction.retry"
  "whyce.economic.enforcement.restriction.deadletter"

  # economic > enforcement > sanction
  "whyce.economic.enforcement.sanction.commands"
  "whyce.economic.enforcement.sanction.events"
  "whyce.economic.enforcement.sanction.retry"
  "whyce.economic.enforcement.sanction.deadletter"

  # constitutional > policy > feedback (enforcement → policy bridge)
  "whyce.constitutional.policy.feedback.events"
  "whyce.constitutional.policy.feedback.deadletter"

  # economic > exchange > fx
  "whyce.economic.exchange.fx.commands"
  "whyce.economic.exchange.fx.events"
  "whyce.economic.exchange.fx.retry"
  "whyce.economic.exchange.fx.deadletter"

  # economic > exchange > rate
  "whyce.economic.exchange.rate.commands"
  "whyce.economic.exchange.rate.events"
  "whyce.economic.exchange.rate.retry"
  "whyce.economic.exchange.rate.deadletter"

  # economic > reconciliation > process (Phase 2 reconciliation batch)
  "whyce.economic.reconciliation.process.commands"
  "whyce.economic.reconciliation.process.events"
  "whyce.economic.reconciliation.process.retry"
  "whyce.economic.reconciliation.process.deadletter"

  # economic > reconciliation > discrepancy (Phase 2 reconciliation batch)
  "whyce.economic.reconciliation.discrepancy.commands"
  "whyce.economic.reconciliation.discrepancy.events"
  "whyce.economic.reconciliation.discrepancy.retry"
  "whyce.economic.reconciliation.discrepancy.deadletter"

  # economic > routing > path (Phase 2 routing batch)
  "whyce.economic.routing.path.commands"
  "whyce.economic.routing.path.events"
  "whyce.economic.routing.path.retry"
  "whyce.economic.routing.path.deadletter"

  # economic > routing > execution (Phase 2 routing batch)
  "whyce.economic.routing.execution.commands"
  "whyce.economic.routing.execution.events"
  "whyce.economic.routing.execution.retry"
  "whyce.economic.routing.execution.deadletter"

  # economic > subject > subject (economic subject bridge)
  "whyce.economic.subject.subject.commands"
  "whyce.economic.subject.subject.events"
  "whyce.economic.subject.subject.retry"
  "whyce.economic.subject.subject.deadletter"

  # economic > compliance > audit (S0 recovery — certification batch 2026-04-16)
  "whyce.economic.compliance.audit.commands"
  "whyce.economic.compliance.audit.events"
  "whyce.economic.compliance.audit.retry"
  "whyce.economic.compliance.audit.deadletter"
)

for TOPIC in "${TOPICS[@]}"; do
  echo "Creating topic: $TOPIC"
  /opt/kafka/bin/kafka-topics.sh \
    --bootstrap-server "$BOOTSTRAP_SERVER" \
    --create \
    --if-not-exists \
    --topic "$TOPIC" \
    --partitions "$PARTITIONS" \
    --replication-factor "$REPLICATION" \
    2>/dev/null || echo "Topic $TOPIC may already exist"
done

echo "All topics created."
