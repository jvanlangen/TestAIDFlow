# AIDFlow AI Development Workflow

Use this workflow for software development work in this repository. It guides
the process alongside this repository's other instructions and existing tool
permissions; it does not authorize commands, deployments or other side effects.

## AIDFlow Entry Point

Before acting or resuming, read [Project](flows/project.mmd). Resolve this path
relative to this `aidflow.md` source URL or local directory. In a consuming
repository this file is the pinned local copy at `.aidflow/method/aidflow.md`; use
that copy and never edit or replace it; version migration is not supported. Resolve subsequent
subflow references relative to their calling source.

Project is the orchestrator and is read only at a phase boundary: when starting
or resuming, and when a subflow returns an outcome that Project must route. While
inside a subflow, follow that subflow and its embedded context; do not reread
Project or other phases. Record the call stack and pending node in the index so
Project can be re-entered exactly. Keep this file, the
[reference files](#reference-files) and all flows from the same source version;
never combine a pinned source with latest flows.

Read the original Mermaid source, not the HTML viewer. A `.mmd` file is a
workflow diagram, not an executable script. Use an available web-fetch tool or a
configured local copy. If Project is inaccessible, report the limitation and
ask for an accessible copy; do not invent its contents.

## Human interaction

Whenever a workflow step requires user communication, artifact review or explicit
approval, read and follow the [Interaction Protocol](reference/interaction.md). It defines
platform-independent interaction semantics; the Mermaid flows remain authoritative
for when each question, review or approval gate occurs.

## Execute Subflows

1. Follow Project's edges and decisions in order. It is the orchestrator, not
   a menu from which to independently select a convenient phase.
2. Treat a node referencing another `.mmd` file as a subflow call. Resolve
   the filename relative to the calling source URL or local file directory.
3. Read that source and follow its steps, decisions and feedback loops. For
   nested calls, apply the same rule and retain the calling step.
4. Pause for explicit human approval at approval decisions. Never approve your
   own work or infer approval merely because an artifact exists.
5. Return the explicit terminal outcome to the caller: success, repair needed,
   deferred, rejected, cancelled or blocked. Follow only its matching edge.
   Only success satisfies completion; other outcomes never imply approval.
   A blocked outcome pauses the affected work until the missing input is resolved.
   Internal failed reviews follow their repair loop before returning.
6. If a referenced file or transition is unclear, stop and ask. A selected
   diagram, viewer breadcrumb or navigation index does not change the entry point.

## Node Context

Any flow may include `%% @aidflow-context <node-id>` comment blocks ending with
`%% @end-aidflow-context`. Read these as Markdown context for the matching
Mermaid node. Use them to understand the node's purpose, focus and expected
outcome; they do not replace the referenced subflow or its approval decisions.
Empty lines inside a context block separate Markdown paragraphs. These comments
are part of the Mermaid source even though they are not rendered in the diagram.

## Workspace and incremental context

In the consuming repository, put only AIDFlow's project output documents—the
Concept, feature Definitions, release scopes and Designs—in `docs/aidflow/`.
This does not move the AIDFlow method: flows, references and instructions remain
in the pinned `.aidflow/method/` copy, and workspace administration remains in
`.aidflow/`. Status, progress, workflow routing, approvals, change events and
verification evidence belong in `.aidflow/index.md`, not in the output documents.
Product decisions and their rationale may remain in the relevant output document;
they do not establish workflow approval or status. The documents in `docs/aidflow/`
are the canonical content records, not generated copies; their paths and digests
belong in the index.
After reading Project, inspect the workspace READMEs and relevant index entries.
Initialize missing files from the [workspace templates](reference/workspace.md);
preserve existing files and ask before adopting or replacing conflicting content.
Read only the active subflow, embedded context and relevant records. Phase record
outlines and transitions live in the flows.

Project documents are AI-managed, not technically read-only. Add the notice from
the workspace templates to each one before review or approval. Users are encouraged
to request changes through the agent, but may edit files directly; filesystem
permissions do not enforce this convention. On resume and before using an indexed
document for a decision or modifying it, compare its exact bytes with its current
indexed digest. If they differ, preserve the file, treat the change as a proposed
change, and follow resume validation before proceeding. Act on a clear change via
the responsible workflow; do not silently overwrite it or assume it is approved.

## Change handling and autonomy

Record old/new digests, classification, rationale, affected IDs, invalidated checks
and approvals, and next state for each change. Editorial changes with no semantic
impact retain approval only through a documented derivation from the approved
version. Material changes invalidate affected approvals; return records to draft
and self-review before reapproval. Treat uncertain meaning changes as material.
Project's impact node owns cross-phase invalidation and repair routes.

Work autonomously within approved scope and fix internal review findings. Escalate
missing product decisions, conflicts, unbounded impact, external blockers,
unauthorized actions or repeated lack of progress with evidence and a focused question.
Blocked is a hold: save prior status and pending node; restore only when invariants
hold, otherwise follow the responsible phase's revision route. Deferral requires
explicit reopening. Subflow outcomes are routing results, not lifecycle statuses.

## Authority and source ownership

Follow the actual instruction hierarchy and tool permissions of the host environment.
AIDFlow cannot override system, developer, organization, repository instructions or
permissions. If a higher-priority instruction disallows an action, stop and report it.

| Information | Authoritative source |
| --- | --- |
| General execution constraints and access | Applicable host and repository instructions |
| Process order, decisions and return paths | `flows/project.mmd` and its referenced subflows |
| Shared status, evidence, resume and change rules | This `aidflow.md` and its [reference files](#reference-files) |
| Approved product intent, feature content and release design | The corresponding approved `docs/aidflow/` records |
| AIDFlow-specific execution rules | This `aidflow.md`, subject to higher-priority instructions |
| Record inventory, lifecycle status and active run | `.aidflow/index.md` |
| Optional issue, board or milestone view | Configured GitHub/GitLab tracker; it does not silently replace `.aidflow/` |

When a tracker disagrees with `.aidflow/index.md`, treat the index as AIDFlow's status
source. Record the discrepancy. If permissions allow, propose reconciling the tracker
to the index; do not silently overwrite tracker data. If a project explicitly assigns
a field to an external system, follow that mapping and record it in `.aidflow/README.md`.

If two applicable instructions conflict, obey the higher-priority instruction. For
conflicts between approved project records, an unresolved index/record mismatch,
or an unmapped tracker disagreement, stop the affected transition, record the
discrepancy, and ask the user. A detectable document edit is handled as a proposed
change under resume validation; it is not by itself an unrecoverable conflict.
Do not silently overwrite a source or infer which one is newer.

## Statuses

Keep three kinds of status separate and use only these values; never invent labels.
All mutable statuses live only in `.aidflow/index.md`, never in content files.

- Record approval (`record_status`): `draft`, `pending-approval`, `approved`,
  `superseded`, `withdrawn`. Only a human causes `approved`; never infer approval.
- Feature and release lifecycle (`feature_status`, `release_status`): owned by the
  Project flow and its subflows.
- Workflow execution (`status`): `ready`, `running`, `waiting-human`, `blocked`,
  `complete`.

Read [Statuses](reference/statuses.md) before changing any status.

## Reference files

Resolve these relative to this `aidflow.md`. Do not load them all up front; read a
file when its trigger occurs, and again if you are unsure of its rules.

| File | Read when |
| --- | --- |
| [`reference/interaction.md`](reference/interaction.md) | Asking a question, presenting an artifact for review, requesting or recording approval, or handling an incomplete answer |
| [`reference/interaction-record.md`](reference/interaction-record.md) | Saving a pending interaction that outlives the session, or resuming one |
| [`reference/concept-example.md`](reference/concept-example.md) | Asking a new project's first Concept questions |
| [`reference/approvals.md`](reference/approvals.md) | Recording, invalidating or checking a human approval |
| [`reference/statuses.md`](reference/statuses.md) | Changing or interpreting any record, feature, release or execution status |
| [`reference/workspace.md`](reference/workspace.md) | Initializing `.aidflow/` or editing `.aidflow/index.md` structure |
| [`reference/resume.md`](reference/resume.md) | Resuming a run or when the workflow version changed |
| [`reference/release-lifecycle.md`](reference/release-lifecycle.md) | Changing a release status, assessing change impact at Project's impact node, or choosing a repair route |

If a required reference file is inaccessible, report the limitation and ask for an
accessible copy; do not guess its rules.
