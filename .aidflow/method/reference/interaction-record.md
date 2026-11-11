# Resumable interaction record

Read this when pausing for a pending interaction that must survive a session, or when resuming one. Types and states are defined in the [Interaction Protocol](interaction.md).

Do not create a separate interaction file for every exchange by default. Reuse the
`.aidflow/index.md`, the relevant project record and the available conversation/review
history. Preserve enough information to resume without repeating questions or
reconstructing decisions:

- interaction ID, type and current interaction state;
- target record ID/path and current content digest, if an artifact is involved;
- the exact unanswered question, requested review, or approval decision and its
   allowed outcomes;
- received response, decision evidence, actor/time when available, and a durable
   reference or exact quoted text with source context;
- workflow source/version, active phase, stable node ID, nested call stack and any
   feature/release arguments;
- prior execution state, next action, blocker/unblock condition, and affected records
   or invalidated approvals/checks.

Update only relevant index entries. Do not copy entire chat transcripts or full
artifacts into the index. If required information is unavailable, state that fact and
keep the interaction waiting or blocked.

When a user response is pending asynchronously, the existing `.aidflow/index.md` may
include this optional section; omit it when there is no pending interaction:

```markdown
## Pending interaction
| ID | Type | State | Target record/digest | Request and allowed outcomes | Response/evidence | Prior node/call stack | Next action or blocker |
| --- | --- | --- | --- | --- | --- | --- | --- |
```

This is a resume pointer, not a second copy of the conversation or artifact. Keep
the complete approval/change evidence in the existing approval-event record and
the actual artifact in its canonical project-document path.
