# Interaction Protocol

This chapter defines how an AIDFlow agent communicates with a user. It is
platform-independent: it specifies what must be communicated and when, not which
chat, terminal, issue tracker, pull request or messaging tool must carry it. It
complements the Mermaid flows; it does not replace them.

## Authority and scope

- [`aidflow.md`](../aidflow.md) is the method entry point and defines how to load context incrementally.
- `flows/project.mmd` determines the current phase and next transition.
- The active subflow and its `@aidflow-context` define phase-specific work and gates.
- This chapter governs Questions, Reviews, Approvals and resumable interactions.
- `.aidflow/` records project-specific facts and current state as defined in [Workspace templates](workspace.md).

Follow the host's instruction hierarchy and tool permissions. This protocol cannot
authorize actions that another applicable instruction or permission disallows. Never
skip a Mermaid transition or approval gate because a communication channel makes it
convenient.

## Interaction types

### Question

Use a Question when required information or a user decision is missing.

1. Inspect the active `.aidflow/` records, relevant previous interactions and active
    subflow before asking. Do not ask for information already available.
2. Ask only focused questions needed for the current phase. In interactive channels,
    ask incrementally when an answer can narrow the next question. In asynchronous
    channels, combine related questions into one concise request.
3. Explain why the answer is needed when that is not self-evident. Offer bounded
    options when the workflow defines them; do not make up choices that change scope.
4. Never invent product requirements, user intent, decisions or approval. A partial
    answer resolves only the questions it actually answers; ask only about what remains.

### Review

Use a Review when an artifact is ready for human evaluation.

1. Identify the artifact path and exact current version (SHA-256 when approval may
    follow). Link it or present the relevant diff/content through the available channel.
2. Summarize what changed, key decisions and assumptions, acceptance coverage, and
    unresolved items or risks. Keep the requested review focused on the artifact's
    purpose and the active phase.
3. Invite specific feedback or a change request. A review request is not an approval.
4. If the user requests changes, incorporate them, update the artifact and digest,
    then request another review when the workflow requires it. Preserve prior review
    evidence as history.

### Approval

Use an Approval only at an approval gate in Project or the active subflow.

1. Name the exact decision, artifact or scope, its path/ID, version and content digest.
2. State the consequence of approval and the valid alternatives (for example revise,
    defer or reject) when those alternatives exist in the active flow.
3. Require an explicit, unambiguous human decision. Silence, unrelated comments,
    agent completion, passing tests, a mergeable branch or a merged pull request do
    not imply approval of an artifact, phase or release. The user must explicitly
    authorize the named target through the current interaction.
4. Record the decision and evidence according to [Approval evidence](approvals.md) and
    `.aidflow/index.md`. Never invent actor, time, source, digest or consent.
5. If the artifact changes materially after approval, use Change handling in [`aidflow.md`](../aidflow.md) to
    determine which approvals/checks are stale before continuing.

## Interaction states and transitions

These states describe the current user interaction, not a feature, release, record
or Mermaid phase. Do not write them into a record's lifecycle-status field. They are
distinct from Workflow execution status in [Statuses](statuses.md).

| State | Meaning | Next action |
| --- | --- | --- |
| `working` | The agent has enough information and authority for the next in-scope step. | Follow the active subflow. |
| `awaiting-input` | A specific answer or user decision is needed before safe progress. | Ask a Question and pause the affected work. |
| `awaiting-review` | A named artifact/version is ready for human evaluation. | Present a Review; do not treat it as approval. |
| `awaiting-approval` | A workflow gate requires explicit authorization for a named artifact/scope/version. | Request Approval and do not cross the gate. |
| `blocked` | A technical, dependency, permission or unresolved procedural issue prevents safe progress. | Record the blocker, prior state and unblock condition; pause affected work. |

Missing product description, requirements or a user decision normally means
`awaiting-input`, not `blocked`. Reserve `blocked` for a non-interaction obstacle
such as unavailable tools/sources, a dependency failure, or a conflict with no
resolvable owner.

### Mapping interaction to execution

| Interaction state | Current execution status in `.aidflow/index.md` |
| --- | --- |
| `working` | `running` while executing, or `ready` when the next step can start |
| `awaiting-input` | `waiting-human` |
| `awaiting-review` | `waiting-human` |
| `awaiting-approval` | `waiting-human` |
| `blocked` | `blocked` |

A pending Question keeps the active subflow and call stack at its waiting node.
It does not return a `blocked` outcome to the caller or set feature/release
lifecycle status to `blocked` merely because an answer is missing. Save the
question and resume pointer; on a sufficient answer, follow the waiting node
back to clarification/review. Partial answers remain `awaiting-input`.
Subflow outcomes route the caller only when the subflow explicitly returns;
they are distinct from interaction and execution statuses.

Valid transitions:

- `working -> awaiting-input` when missing information prevents the next valid step.
- `awaiting-input -> working` only when the answer is sufficient; otherwise remain
   `awaiting-input` and ask only the unresolved part.
- `working -> awaiting-review` when the reviewable artifact is ready.
- `awaiting-review -> working` when feedback requires revision, or when review is
   complete and no separate approval gate is required.
- `awaiting-review -> awaiting-approval` only when the review is complete and the
   workflow requires a separate explicit decision.
- `working` or `awaiting-review -> awaiting-approval` when an explicit approval gate
   is reached and its exact target is ready.
- `awaiting-approval -> working` only after an explicit approval and successful
   validation of its evidence against the current artifact digest.
- `awaiting-approval -> working` after requested changes only by returning the artifact
   to draft/review through the active flow; the prior approval was not granted.
- A defer, reject or cancel follows only the matching Mermaid outcome. If no matching
   edge exists, transition to `blocked` and ask rather than inventing a route.
- Any active interaction may become `blocked` when a non-question obstacle prevents
   safe progress. After resolution, restore the saved prior state only if its inputs,
   approvals and evidence remain valid; otherwise follow the required revision route.

No response leaves the interaction in its current awaiting state. Do not keep running
past a human boundary or convert a timeout into consent.

## Interactive and asynchronous channels

The protocol is identical in both kinds of environment:

- **Interactive:** ask in chat/terminal/IDE, accept the reply, update the relevant
   record, then continue only along the valid workflow edge.
- **Asynchronous:** publish the same Question, Review or Approval request in an
   available channel, save the pending interaction and next action, then stop. On a
   later session, inspect the repository state and interaction evidence before
   resuming. Do not assume a PR comment, issue update or merge is approval by itself.

Examples include GitHub Copilot Coding Agent comments/reviews, Copilot CLI terminal
interaction, VS Code Copilot chat, and Claude Code terminal/IDE interaction. Other
agents can use any supported channel. None is mandatory; the channel transports the
interaction but does not change its meaning or approval requirements.

## Saving a pending interaction

When a response is pending beyond the current session, read the
[resumable interaction record](interaction-record.md) before saving or resuming it.

## Incomplete answers, changes and conflicts

- **Incomplete answer:** incorporate only what was answered, retain the unresolved
   items and ask a focused follow-up. Do not assume the missing parts.
- **Change request during review:** revise the draft, preserve the old version, and
   resubmit the changed artifact for review/approval as required.
- **Rejection or deferral:** record the user's decision and follow the active flow's
   rejected/deferred edge. Do not reopen automatically.
- **Change to approved requirements:** classify its impact using Change handling in [`aidflow.md`](../aidflow.md); stale only dependent approvals and checks, preserve unaffected evidence,
   and resume at the earliest affected phase. If impact is unclear, stop and ask.
- **Instruction or record conflict:** obey the host instruction hierarchy and the
   Authority and source ownership rules in [`aidflow.md`](../aidflow.md). Do not silently overwrite a source. Ask only when
   explicit ownership and existing rules cannot resolve the conflict.
- **Unrelated instruction:** never interpret an unrelated user request as approval to
   bypass a pending interaction, phase gate or repository permission.
