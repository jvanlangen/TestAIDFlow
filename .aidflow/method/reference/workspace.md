# `.aidflow/README.md` template

Initialize the project workspace with this notice, preserving any additional
project instructions that do not conflict with [`aidflow.md`](../aidflow.md):

```markdown
# AIDFlow project workspace

This directory contains AI-managed project records used by AIDFlow. It is not a
general-purpose documentation folder.

Do not edit these records directly. Ask the AIDFlow agent to update them through
the workflow, then review and approve proposed changes before they are merged.

Do not store secrets or credentials here.
```

# `.aidflow/index.md` inventory and resume template

The index is the canonical inventory and resume point, not a copy of the records.
Use this structure and keep detailed content in its individual files.
Never register `.aidflow/index.md` in its own Records table or store its own file
digest inside it: updating that digest would change the file again. The index is
validated through its structure, record/evidence consistency and available Git
history; do not treat Git history as human approval. `.aidflow/README.md` may be an
operational record with a current digest and `record_status: none`.

Use stable project-local IDs: `F-0001` for features, the user-visible version string
(for example `1.2.0`) for releases, and `A-0001` for approval/change events. Never
reuse an ID.

```markdown
# AIDFlow project index

## Workflow source
- entry: <`.aidflow/method/aidflow.md`, or the source URL when no local copy exists>

| Source path | Role | Ref (commit/tag) | SHA-256 |
| --- | --- | --- | --- |
| <one row per aidflow.md, flow or reference/*.md file read> | <source role> | <immutable ref or unpinned> | <digest or unknown> |

## Tracker integration
- provider: <none|github|gitlab>
- project_or_board: <reference or none>

## Current execution
- status: <ready|running|waiting-human|blocked|complete>
- phase: <stable subflow ID>
- node_id: <stable Mermaid node ID>
- call_stack: <caller node -> subflow node -> current node>
- call_arguments: <feature/release mode, target IDs, new-work/rework, findings>
- pending_targets: <affected feature IDs in dependency order or none>
- feature_id: <ID or none>
- release_id: <version or none>
- next_action: <specific action, decision or evidence needed>

## Records
| ID | Type | Path | record_status | Current SHA-256 | Approved SHA-256 | Depends on | Tracker reference |
| --- | --- | --- | --- | --- | --- | --- | --- |

## Features
| Feature ID | Definition record ID | feature_status | Active release ID or none | Prior status if blocked | Event reference |
| --- | --- | --- | --- | --- | --- |

## Releases
| Release ID | Scope record ID | Design record ID or none | release_status | Prior status if blocked | Event reference |
| --- | --- | --- | --- | --- | --- |

Record IDs are unique across Records. Feature and release IDs identify their own
entity tables and reference Records by ID; do not duplicate lifecycle statuses in
Records or content files. Use `none` only for explicitly non-applicable fields.

## Approval and change events
| Event ID | Kind: decision/change | Record/entity | Old/new SHA-256 | Decision or change classification | Actor | Timestamp | Evidence and rationale | Affected records/checks/events | Next state |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

## Verification evidence
| Feature/release | Check | Result | Evidence path/reference | Digest | Current/stale and reason |
| --- | --- | --- | --- | --- | --- |
```

## Workflow source registration

In a consuming repository the method is a pinned local copy in `.aidflow/method/`,
made at setup from one immutable source commit (see `setup.md`). Register every
copied file in the index at setup with its relative path, the source commit and the
SHA-256 of its exact bytes. The local copy is the source of truth for the run; the
rules below apply when a copy or version must be established or compared.

At initialization, prefer resolving the entry source to one immutable commit and
record it; resolve tags to their commit when possible. Re-read the entry file at
that ref before using it, so an earlier read from `main` is not mixed into the run.
Read all flows and reference
files from that same version, including files first needed later in the run.
Register each file when read, with its path, role, ref and SHA-256 of the exact
bytes. This includes `aidflow.md`, Project, each used subflow and each used
`reference/*.md`; do not load unused reference files just to populate the table.

If an immutable ref is unavailable, record `unpinned` and the available digests.
Keep a consistent local snapshot or obtain a version-consistent copy before
loading another file; do not silently add a latest file to an older snapshot.
Unknown provenance or missing digests are not proof of a consistent version.
Follow [resume validation](resume.md) when consistency cannot be established.

Use stable node IDs in every Mermaid flow and keep them stable across compatible
workflow revisions. If an ID must change, document an old-to-new mapping in the new
workflow version. Store the source-level Mermaid ID (for example `definitionApproval`),
not a generated SVG DOM ID whose numeric suffix can change when nodes are inserted.
Use `unknown` or `none` where the information is genuinely unavailable; never fill
a field from assumption.
