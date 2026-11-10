# Record and lifecycle statuses

Keep record approval status separate from feature/release lifecycle status and from
the workflow execution phase. Use only these values; do not invent new status labels.

## Record approval status

| Status | Meaning and allowed transition |
| --- | --- |
| `draft` | AI or user is preparing a record. May transition to `pending-approval` or `withdrawn`. |
| `pending-approval` | Exact content version is awaiting a human decision. May transition to `approved`, `draft` for requested changes, or `withdrawn`. |
| `approved` | A human approved the recorded content digest. Content changes follow Change handling in [`aidflow.md`](../aidflow.md). |
| `superseded` | Replaced by a newer record/version. Preserve the old file or history and its evidence. |
| `withdrawn` | Explicitly withdrawn by the user. Do not resume it without a new user request. |

Only a human can cause an approval transition to `approved`. The AI may prepare
records, request approval, and update operational statuses when supported by evidence;
it must not approve its own work or infer approval from silence, mergeability, tests,
or existence of an artifact.

## Status ownership

All mutable statuses live only in `.aidflow/index.md`, never in the content files.
Use separate tables so a release has exactly one lifecycle status:

| Entity / index table | Field | Meaning |
| --- | --- | --- |
| Concept, Definition, scope and Design / Records | `record_status` | Approval state of that content version. |
| README / Records | `record_status: none` | Operational files; `none` means not applicable, not approval. |
| Feature / Features | `feature_status` | Delivery state of that feature ID. |
| Release / Releases | `release_status` | Delivery state of the whole version, never of an individual file. |
| Current execution | `status` | Whether the current workflow action can run. |

The index itself is not a Records entry and has no self-digest. See the
[workspace rules](workspace.md) for its integrity and history.

A Design uses `record_status: draft`, `pending-approval` or `approved`; it never
uses these values as `release_status`. A release can be `design-pending-approval`
while its Design record is still being revised in `draft`. Lifecycle status alone
never proves that an artifact is approved or ready to present.

## Record revision

For every approval-gated record, material revision or invalidated approval moves
`approved -> draft`. Self-review then permits `draft -> pending-approval`; only
explicit approval permits `pending-approval -> approved`. Requested changes move
`pending-approval -> draft`. Preserve earlier versions and their approval events.
Editorial descendants follow Change handling in [`aidflow.md`](../aidflow.md) without a new approval.

A human may withdraw an active record (`draft`, `pending-approval` or `approved`)
into `withdrawn`; reopening explicitly moves it to `draft`. Mark a historical
version `superseded` only when a replacement exists; this is terminal for that
historical version. Neither withdrawal nor supersession approves a replacement.

## Workflow execution status

Execution status is separate from all record and feature/release statuses. Use only:

- `ready`: next action is known and can proceed under the workflow.
- `running`: an authorized subflow is in progress.
- `waiting-human`: user input, artifact review or explicit approval is required.
- `blocked`: a conflict, missing evidence, unavailable source or external blocker
  prevents safe continuation.
- `complete`: the relevant subflow reached its successful terminal node.

Do not use a record status such as `approved` to imply the current workflow phase, or
an execution status such as `running` to imply record approval.
