#!/usr/bin/env python3
"""β #1 D7 fix: bulk-add IHasAggregateId to every command record.

For each shared/contracts/**/*.cs file containing `public sealed record XxxCommand(...)`:
  1. Extract the first Guid parameter (treat as aggregate id).
  2. Add `: IHasAggregateId { public Guid AggregateId => <id>; }` to the record.
  3. Add `using Whycespace.Shared.Contracts.Runtime;` if missing.

Skips commands whose first parameter is not a Guid (manual review required).

Reports a summary of edits + skips.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).parent.parent / "src" / "shared" / "contracts"
COMMAND_HEADER = re.compile(
    r"public\s+sealed\s+record\s+(\w+Command)\s*\(",
    re.MULTILINE,
)
USING_LINE = "using Whycespace.Shared.Contracts.Runtime;"

edited = []
skipped_non_guid = []
already_implements = []
files_changed = 0


def find_record_close(text: str, open_paren: int) -> int:
    depth = 0
    for i in range(open_paren, len(text)):
        if text[i] == "(":
            depth += 1
        elif text[i] == ")":
            depth -= 1
            if depth == 0:
                return i
    return -1


def first_guid_param(param_block: str) -> str | None:
    # comma-split; for each token, take the first whitespace-separated pair.
    for raw in param_block.split(","):
        clean = re.sub(r"\[[^\]]*\]", "", raw).strip()
        if not clean:
            continue
        # type and name; type may include namespaces but commands here
        # don't use generics in their constructor parameter list.
        parts = clean.split(None, 1)
        if len(parts) < 2:
            continue
        type_name, ident = parts[0].strip(), parts[1].strip().split("=", 1)[0].strip()
        if type_name == "Guid":
            return ident
    return None


def process_file(path: Path) -> bool:
    text = path.read_text(encoding="utf-8")
    original = text
    matches = list(COMMAND_HEADER.finditer(text))
    if not matches:
        return False

    # Build edits in REVERSE order so indexes stay stable.
    edits: list[tuple[int, int, str]] = []
    for m in reversed(matches):
        name = m.group(1)
        open_paren = text.find("(", m.end() - 1)
        close_paren = find_record_close(text, open_paren)
        if close_paren < 0:
            continue
        # Look at what follows close_paren: `;`, `{`, or `: <inheritance>`.
        body_start = next(
            (i for i in range(close_paren + 1, len(text)) if text[i] in (";", "{", ":")),
            -1,
        )
        if body_start < 0:
            continue
        # If `:` then inheritance clause exists; check if IHasAggregateId is in it.
        if text[body_start] == ":":
            end_of_inheritance = next(
                (i for i in range(body_start, len(text)) if text[i] in (";", "{")),
                -1,
            )
            if end_of_inheritance < 0:
                continue
            inh = text[body_start:end_of_inheritance]
            if "IHasAggregateId" in inh:
                already_implements.append(f"{path}: {name}")
                continue
            # Has different inheritance clause — skip with a warning.
            skipped_non_guid.append(f"{path}: {name} (existing inheritance: {inh.strip()})")
            continue

        # No inheritance yet — extract first Guid param and inject.
        param_block = text[open_paren + 1 : close_paren]
        ident = first_guid_param(param_block)
        if ident is None:
            skipped_non_guid.append(f"{path}: {name} (no Guid first-param)")
            continue

        # Build inheritance suffix between close_paren and body_start.
        between = text[close_paren + 1 : body_start]
        injection = (
            f" : IHasAggregateId\n"
            f"{{\n"
            f"    public Guid AggregateId => {ident};\n"
            f"}}"
        )
        # Replace `;` body marker with `{ AggregateId => ... }` block.
        if text[body_start] == ";":
            edits.append((close_paren + 1, body_start + 1, between + injection))
        elif text[body_start] == "{":
            # The record already has a body; insert AggregateId BEFORE the
            # opening brace and merge.
            edits.append(
                (close_paren + 1, body_start + 1, between + " : IHasAggregateId\n{\n    public Guid AggregateId => " + ident + ";\n")
            )

        edited.append(f"{path}: {name} -> AggregateId => {ident}")

    if not edits:
        return False
    for start, end, replacement in edits:
        text = text[:start] + replacement + text[end:]

    # Inject `using Whycespace.Shared.Contracts.Runtime;` if missing.
    if USING_LINE not in text:
        # Insert after the last existing `using` line, or at top.
        last_using = list(re.finditer(r"^using\s+[\w\.]+;\s*$", text, re.MULTILINE))
        if last_using:
            insert_pos = last_using[-1].end()
            text = text[:insert_pos] + "\n" + USING_LINE + text[insert_pos:]
        else:
            text = USING_LINE + "\n\n" + text

    if text != original:
        path.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    global files_changed
    for cs in ROOT.rglob("*.cs"):
        if "obj" in cs.parts or "bin" in cs.parts:
            continue
        if process_file(cs):
            files_changed += 1

    print(f"Files changed:    {files_changed}")
    print(f"Records edited:   {len(edited)}")
    print(f"Already implements: {len(already_implements)}")
    print(f"Skipped:          {len(skipped_non_guid)}")
    if skipped_non_guid:
        print("\nSKIPPED (manual review needed):")
        for s in skipped_non_guid[:30]:
            print(f"  {s}")


if __name__ == "__main__":
    main()
